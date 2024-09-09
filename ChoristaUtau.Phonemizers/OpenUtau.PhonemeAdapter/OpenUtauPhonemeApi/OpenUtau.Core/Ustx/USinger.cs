﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using OpenUtau.Classic;
using OpenUtau.Core.Util;

namespace OpenUtau.Core.Ustx {
    public class UOto : INotifyPropertyChanged {
        public virtual string Alias { get; private set; }
        public virtual string Phonetic { get; private set; }
        public virtual string Set { get; private set; }
        public virtual string Color { get; private set; }
        public virtual string Prefix { get; private set; }
        public virtual string Suffix { get; private set; }
        public virtual SortedSet<int> ToneSet { get; private set; }
        public virtual string File { get; private set; }
        public virtual string DisplayFile { get; private set; }
        public virtual double Offset {
            get => offset;
            set {
                offset = Math.Max(0, Math.Round(value, 3));
                NotifyPropertyChanged(nameof(Offset));
            }
        }
        public virtual double Consonant {
            get => consonant;
            set {
                consonant = Math.Max(0, Math.Round(value, 3));
                NotifyPropertyChanged(nameof(Consonant));
            }
        }
        public virtual double Cutoff {
            get => cutoff;
            set {
                cutoff = Math.Round(value, 3);
                NotifyPropertyChanged(nameof(Cutoff));
            }
        }
        public virtual double Preutter {
            get => preutter;
            set {
                preutter = Math.Max(0, Math.Round(value, 3));
                NotifyPropertyChanged(nameof(Preutter));
            }
        }
        public virtual double Overlap {
            get => overlap;
            set {
                overlap = Math.Round(value, 3);
                NotifyPropertyChanged(nameof(Overlap));
            }
        }
        public virtual List<string> SearchTerms { get; private set; }

        public virtual event PropertyChangedEventHandler PropertyChanged;

        private Oto oto;
        private double offset;
        private double consonant;
        private double cutoff;
        private double preutter;
        private double overlap;

        public UOto() { }

        public UOto(Oto oto, UOtoSet set, USubbank subbank) {
            this.oto = oto;
            Alias = oto.Alias;
            Phonetic = oto.Phonetic;
            Set = set.Name;
            Color = subbank?.Color;
            Prefix = subbank?.Prefix;
            Suffix = subbank?.Suffix;
            ToneSet = subbank?.toneSet;
            if (!string.IsNullOrEmpty(oto.Wav)) {
                File = Path.Combine(set.Location, oto.Wav);
            } else {
                File = string.Empty;
            }
            DisplayFile = oto?.Wav;
            Offset = oto.Offset;
            Consonant = oto.Consonant;
            Cutoff = oto.Cutoff;
            Preutter = oto.Preutter;
            Overlap = oto.Overlap;

            SearchTerms = new List<string>();
        }

        public static UOto OfDummy(string alias) => new UOto() {
            Alias = alias,
            Phonetic = alias,
        };

        public virtual void WriteBack() {
            oto.Offset = offset;
            oto.Consonant = consonant;
            oto.Cutoff = cutoff;
            oto.Preutter = preutter;
            oto.Overlap = overlap;
        }

        private void NotifyPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString() => Alias;
    }

    public class UOtoSet {
        public string Name => otoSet.Name;
        public readonly string Location;

        private readonly OtoSet otoSet;

        public UOtoSet(OtoSet otoSet, string singersPath) {
            this.otoSet = otoSet;
            if (!string.IsNullOrEmpty(otoSet.File)) {
                Location = Path.Combine(singersPath, Path.GetDirectoryName(otoSet.File));
            } else {
                Location = string.Empty;
            }
        }

        public override string ToString() => Name;
    }

    public class USubbank {
        public virtual string Color {
            get => subbank.Color;
            set => subbank.Color = value;
        }
        public virtual string Prefix {
            get => subbank.Prefix;
            set => subbank.Prefix = value;
        }
        public virtual string Suffix {
            get => subbank.Suffix;
            set => subbank.Suffix = value;
        }
        public virtual string ToneRangesString {
            get => toneRangesString;
            set {
                subbank.ToneRanges = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                toneRangesString = value;
            }
        }

        public readonly SortedSet<int> toneSet;
        public readonly Subbank subbank;

        private string toneRangesString;

        public USubbank(Subbank subbank) {
            this.subbank = subbank;
            toneSet = new SortedSet<int>();
            if (subbank.ToneRanges != null) {
                toneRangesString = string.Join(',', subbank.ToneRanges);
                foreach (var range in subbank.ToneRanges) {
                    AddToneRange(range, toneSet);
                }
            } else {
                toneRangesString = string.Empty;
            }
        }

        private static void AddToneRange(string range, SortedSet<int> set) {
            var parts = range.Split('-');
            if (parts.Length == 1) {
                int tone = MusicMath.NameToTone(parts[0]);
                if (tone > 0) {
                    set.Add(tone);
                }
            } else if (parts.Length == 2) {
                int start = MusicMath.NameToTone(parts[0]);
                int end = MusicMath.NameToTone(parts[1]);
                if (start > 0 && end > 0) {
                    for (int i = start; i <= end; ++i) {
                        set.Add(i);
                    }
                }
            }
        }
    }

    [Flags] public enum USingerType { Classic = 0x1 }

    public class USinger : INotifyPropertyChanged, IEquatable<USinger> {
        protected static readonly List<UOto> emptyOtos = new List<UOto>();

        public virtual string Id { get; }
        public virtual string Name => name;
        public virtual Dictionary<string, string> LocalizedNames { get; }
        public virtual USingerType SingerType { get; }
        public virtual string BasePath { get; }
        public virtual string Author { get; }
        public virtual string Voice { get; }
        public virtual string Location { get; }
        public virtual string Web { get; }
        public virtual string Version { get; }
        public virtual string OtherInfo { get; }
        public virtual IList<string> Errors { get; }
        public virtual string Avatar { get; }
        public virtual byte[] AvatarData { get; }
        public virtual string Portrait { get; }
        public virtual float PortraitOpacity { get; }
        public virtual int PortraitHeight { get; }
        public virtual string Sample { get; }
        public virtual string DefaultPhonemizer { get; }
        public virtual Encoding TextFileEncoding => Encoding.UTF8;
        public virtual IList<USubbank> Subbanks { get; }
        public virtual IList<UOto> Otos => emptyOtos;

        public bool Found => found;
        public bool Loaded => found && loaded;
        public bool OtoDirty {
            get => otoDirty;
            set {
                otoDirty = value;
                NotifyPropertyChanged(nameof(OtoDirty));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool found;
        protected bool loaded;
        protected bool otoDirty;

        private string name;

        public string LocalizedName { 
            get {
                    return Found ? Name : $"[Missing] {Name}";
            }
        }

        public virtual void EnsureLoaded() { }
        public virtual void Reload() { }
        public virtual void Save() { }
        public virtual bool TryGetOto(string phoneme, out UOto oto) {
            oto = default;
            return false;
        }
        public virtual bool TryGetMappedOto(string phoneme, int tone, out UOto oto) {
            return TryGetOto(phoneme, out oto);
        }
        public virtual bool TryGetMappedOto(string phoneme, int tone, string color, out UOto oto) {
            return TryGetOto(phoneme, out oto);
        }

        public virtual IEnumerable<UOto> GetSuggestions(string text) { return emptyOtos; }
        public virtual byte[] LoadPortrait() => null;
        public virtual byte[] LoadSample() => null;
        public override string ToString() => LocalizedName;
        public bool Equals(USinger other) {
            // Tentative: Since only the singer's Id is recorded in ustx and preferences, singers with the same Id are considered identical.
            // Singer with the same directory name in different locations may be identical.
            if (other != null && other.Id == this.Id) {
                return true;
            } else {
                return false;
            }
        }
        public override int GetHashCode() => Id.GetHashCode();

        public static USinger CreateMissing(string name) {
            return new USinger() {
                found = false,
                loaded = false,
                name = name,
            };
        }

        private void NotifyPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Some types of singers store their data in memory when rendering.
        /// This method is called when the singer is no longer used.
        /// Note:
        /// - the voicebank may be used again even after this method is called.
        /// - this method may be called even when the singer has not been used
        /// </summary>
        public virtual void FreeMemory(){ }
    }
}
