using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using OpenUtau.Api;
using OpenUtau.Plugin.Builtin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static OpenUtau.Api.Phonemizer;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter
{
    public static class OpenUtauPhonemizerInit
    {
        public static void InitPhonemizer(this Phonemizer openUtauPhonemizer, VoiceBank vb)
        {
            var singer = new ChoristaSinger(vb);
            var timing = new Timeing125Adapter();
            openUtauPhonemizer.Testing = true;
            openUtauPhonemizer.SetSinger(singer);
            openUtauPhonemizer.SetTiming(timing);
            openUtauPhonemizer.Testing = false;
        }
    }
    public class PhonemizerProcessedAdapter(Phonemizer openUtauPhonemizer)
    {
        private Note? transNote(UMidiNote? note)
        {
            if (note == null) return null;
            if (note.Lyric == "R") return null;
            var n = new Note() { position = (int)Math.Round(note.StartMSec),
                duration = (int)Math.Round(note.DurationMSec),
                lyric = note.Lyric,
                tone = note.NoteNumber,
                phoneticHint = ""
            };
            if(note.Lyric.Contains("[") && note.Lyric.Contains("]"))
            {
                Match match = Regex.Match(note.Lyric, @"^(.*?)\[(.*?)\]$");
                if (match.Success)
                {
                    n.lyric = match.Groups[1].Value;
                    n.phoneticHint = match.Groups[2].Value;
                }
            }
            return n;
        }
        private Result? BuildSyllable(UMidiNote?[] grp,out Note? processedNote)
        {
            var cNote = transNote(grp[1]);
            processedNote = cNote;
            if (cNote == null) return null;
            var pNote = transNote(grp[0]);
            var nNote = transNote(grp[2]);
            var res = openUtauPhonemizer.Process([(Note)cNote], pNote, nNote, pNote, nNote, pNote == null ? new Note[0] : new Note[1] { (Note)pNote });
            return res;
        }
        public List<UPhonemeNote> Process(UMidiPart MidiPart, int NoteIndex)
        {
            //Default is Link On NextNote,As Vowel STP;
            UMidiNote?[] GroupCurrent;
            UMidiNote?[] GroupNext;
            {
                UMidiNote? prevNote = NoteIndex > 0 ? MidiPart.Notes[NoteIndex - 1] : null;
                UMidiNote? curNote = MidiPart.Notes[NoteIndex];
                UMidiNote? nextNote = (NoteIndex + 1) < MidiPart.Notes.Count ? MidiPart.Notes[NoteIndex + 1] : null;
                UMidiNote? nextNextNote = (NoteIndex + 2) < MidiPart.Notes.Count ? MidiPart.Notes[NoteIndex + 2] : null;

                GroupCurrent = [
                   prevNote,curNote,nextNote
                ];
                GroupNext = [
                   curNote,nextNote,nextNextNote
                ];

            }

            Result? SyllableCurrent = BuildSyllable(GroupCurrent,out Note? ProcessNoteCurrent);
            Result? SyllableNext = BuildSyllable(GroupNext, out Note? ProcessNoteNext);

            if (SyllableCurrent == null) return new List<UPhonemeNote>();

            List<Phoneme> OUPhoneme = new List<Phoneme>();
            var procNote = (Note)ProcessNoteCurrent;
            foreach(var pi in ((Result)SyllableCurrent).phonemes)
            {
                if(pi.position>=0)
                {
                    Phoneme p = new Phoneme()
                    {
                        position = pi.position + procNote.position,
                        index = OUPhoneme.Count,
                        phoneme = pi.phoneme
                    };
                    OUPhoneme.Add(p);
                }
            }
            int EndingTime = -1;
            if (SyllableNext != null)
            {
                procNote = (Note)ProcessNoteNext;
                foreach (var pi in ((Result)SyllableNext).phonemes)
                {
                    if (pi.position >= 0)
                    {
                        EndingTime = pi.position + procNote.position;
                        break;
                    }
                    {
                        Phoneme p = new Phoneme()
                        {
                            position = pi.position + procNote.position,
                            index = OUPhoneme.Count,
                            phoneme = pi.phoneme
                        };
                        OUPhoneme.Add(p);
                    }
                }
            }
            EndingTime=(EndingTime>0 && EndingTime > OUPhoneme.Last().position)?
                        EndingTime:
                        Math.Max(OUPhoneme.Last().position + 120,((Note)ProcessNoteCurrent).position+ ((Note)ProcessNoteCurrent).duration);


            List<UPhonemeNote> ret = new List<UPhonemeNote>();
            for(int i = 0; i < OUPhoneme.Count; i++)
            {
                int endTime = i + 1 < OUPhoneme.Count ? OUPhoneme[i + 1].position : EndingTime;
                ret.Add(new UPhonemeNote(MidiPart.Notes[NoteIndex], OUPhoneme[i].phoneme, endTime - OUPhoneme[i].position));

            }
            return ret;
        }
    }
}
