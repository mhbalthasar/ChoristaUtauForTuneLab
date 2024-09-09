using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenUtau.Api;
using Serilog;
using SharpCompress;
using YamlDotNet.Serialization;

namespace OpenUtau.Core.Ustx {
    public abstract class UPart {
        public string name = "New Part";
        public string comment = string.Empty;
        public int trackNo;
        public int position = 0;

        [YamlIgnore] public virtual string DisplayName { get; }
        [YamlIgnore] public virtual int Duration { set; get; }
        [YamlIgnore] public int End { get { return position + Duration; } }

        public UPart() { }

        public abstract int GetMinDurTick(UProject project);

        public virtual void BeforeSave(UProject project, UTrack track) { }
        public virtual void AfterLoad(UProject project, UTrack track) { }

        public virtual void Validate(ValidateOptions options, UProject project, UTrack track) { }

        public abstract UPart Clone();
    }

    public class UVoicePart : UPart {
        public int duration;

        [YamlMember(Order = 100)]
        public SortedSet<UNote> notes = new SortedSet<UNote>();
        [YamlMember(Order = 101)]
        public List<UCurve> curves = new List<UCurve>();

        [YamlIgnore] public List<UPhoneme> phonemes = new List<UPhoneme>();
        [YamlIgnore] public int phonemesRevision = 0;

        [YamlIgnore] private long notesTimestamp;
        [YamlIgnore] private long phonemesTimestamp;

        [YamlIgnore] public bool PhonemesUpToDate => notesTimestamp == phonemesTimestamp;

        public override string DisplayName => name;
        public override int Duration { get => duration; set => duration = value; }

        public override int GetMinDurTick(UProject project) {
            int endTicks = position + (notes.LastOrDefault()?.End ?? 1);
            project.timeAxis.TickPosToBarBeat(endTicks, out int bar, out int beat, out int remainingTicks);
            return project.timeAxis.BarBeatToTickPos(bar, beat + 1) - position;
        }

        public override void BeforeSave(UProject project, UTrack track) {
            foreach (var note in notes) {
                note.BeforeSave(project, track, this);
            }
        }

        public override void AfterLoad(UProject project, UTrack track) {
            foreach (var note in notes) {
                note.AfterLoad(project, track, this);
            }
            Duration = Math.Max(Duration, GetMinDurTick(project));
            foreach (var curve in curves) {
                if (project.expressions.TryGetValue(curve.abbr, out var descriptor)) {
                    curve.descriptor = descriptor;
                }
            }
        }

        public override void Validate(ValidateOptions options, UProject project, UTrack track) {
            UNote lastNote = null;
            foreach (UNote note in notes) {
                note.Prev = lastNote;
                note.Next = null;
                if (lastNote != null) {
                    lastNote.Next = note;
                }
                lastNote = note;
            }
            foreach (UNote note in notes) {
                note.ExtendedDuration = note.duration;
                if (note.Prev != null && note.Prev.End == note.position && note.lyric.StartsWith("+")) {
                    note.Extends = note.Prev.Extends ?? note.Prev;
                    note.Extends.ExtendedDuration = note.End - note.Extends.position;
                } else {
                    note.Extends = null;
                }
            }
            foreach (UNote note in notes) {
                note.Validate(options, project, track, this);
            }
            if (!options.SkipPhonemizer) {
                var noteIndexes = new List<int>();
                var groups = new List<OpenUtau.Api.Phonemizer.Note[]>();
                int noteIndex = 0;
                foreach (var note in notes) {
                    if (note.OverlapError || note.Extends != null) {
                        noteIndex++;
                        continue;
                    }
                    var group = new List<UNote>() { note };
                    var next = note.Next;
                    while (next != null && next.Extends == note) {
                        group.Add(next);
                        next = next.Next;
                    }
                    groups.Add(group.Select(e => e.ToPhonemizerNote(track, this)).ToArray());
                    noteIndexes.Add(noteIndex);
                    noteIndex++;
                }
            }
            if (!options.SkipPhoneme) {
                UPhoneme lastPhoneme = null;
                foreach (var phoneme in phonemes) {
                    phoneme.Prev = lastPhoneme;
                    phoneme.Next = null;
                    if (lastPhoneme != null) {
                        lastPhoneme.Next = phoneme;
                    }
                    lastPhoneme = phoneme;
                }
                foreach (var note in notes) {
                    for (int i = note.phonemeOverrides.Count - 1; i >= 0; --i) {
                        if (note.phonemeOverrides[i].IsEmpty) {
                            note.phonemeOverrides.RemoveAt(i);
                        }
                    }
                }
                foreach (var phoneme in phonemes) {
                    phoneme.position = phoneme.rawPosition;
                    phoneme.phoneme = phoneme.rawPhoneme;
                    phoneme.preutterDelta = null;
                    phoneme.overlapDelta = null;
                    var note = phoneme.Parent;
                    if (note == null) {
                        continue;
                    }
                    var o = note.phonemeOverrides.FirstOrDefault(o => o.index == phoneme.index);
                    if (o != null) {
                        phoneme.position += o.offset ?? 0;
                        phoneme.phoneme = !string.IsNullOrWhiteSpace(o.phoneme) ? o.phoneme : phoneme.rawPhoneme;
                        phoneme.preutterDelta = o.preutterDelta;
                        phoneme.overlapDelta = o.overlapDelta;
                    }
                }
                // Safety treatment after phonemizer output and phoneme overrides.
                for (int i = phonemes.Count - 2; i >= 0; --i) {
                    phonemes[i].position = Math.Min(phonemes[i].position, phonemes[i + 1].position - 10);
                }
                foreach (var phoneme in phonemes) {
                    var note = phoneme.Parent;
                    if (note == null) {
                        continue;
                    }
                    phoneme.Validate(options, project, track, this, note);
                }
            }
        }

        public override UPart Clone() {
            return new UVoicePart() {
                name = name,
                comment = comment,
                trackNo = trackNo,
                position = position,
                notes = new SortedSet<UNote>(notes.Select(note => note.Clone())),
                curves = curves.Select(c => c.Clone()).ToList(),
                Duration = Duration,
            };
        }
    }

    public class UWavePart : UPart {
        string _filePath;

        [YamlIgnore]
        public string FilePath {
            set {
                _filePath = value;
                name = Path.GetFileName(value);
            }
            get { return _filePath; }
        }

        [YamlMember(Order = 100)] public string relativePath;
        [YamlMember(Order = 101)] public double fileDurationMs;
        [YamlMember(Order = 102)] public double skipMs;
        [YamlMember(Order = 103)] public double trimMs;

        [YamlIgnore]
        public override string DisplayName => Missing ? $"[Missing] {name}" : name;
        [YamlIgnore]
        public override int Duration {
            get => duration;
            set { }
        }
        [YamlIgnore] bool Missing { get; set; }
        [YamlIgnore] public float[] Samples { get; private set; }

        [YamlIgnore] public int channels;
        [YamlIgnore] public int sampleRate;
        [YamlIgnore] public int peaksSampleRate;

        private int duration;

        public override int GetMinDurTick(UProject project) {
            double posMs = project.timeAxis.TickPosToMsPos(position);
            int end = project.timeAxis.MsPosToTickPos(posMs + fileDurationMs);
            return end - position;
        }

        public override UPart Clone() {
            return null;
        }

        public void Load(UProject project) {
            
        }

        public override void Validate(ValidateOptions options, UProject project, UTrack track) {
            UpdateDuration(project);
        }

        private void UpdateDuration(UProject project) {
            double posMs = project.timeAxis.TickPosToMsPos(position);
            int end = project.timeAxis.MsPosToTickPos(posMs + fileDurationMs);
            duration = end - position;
        }

        public override void BeforeSave(UProject project, UTrack track) {
            relativePath = Path.GetRelativePath(Path.GetDirectoryName(project.FilePath), FilePath);
        }

        public override void AfterLoad(UProject project, UTrack track) {
            try {
                FilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(project.FilePath), relativePath ?? ""));
            } catch {
                if (string.IsNullOrWhiteSpace(FilePath)) {
                    throw;
                }
            }
            Load(project);
        }
    }
}
