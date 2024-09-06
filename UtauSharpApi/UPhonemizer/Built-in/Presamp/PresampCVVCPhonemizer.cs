using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomlyn.Model;
using Tomlyn;
using UtauSharpApi.UNote;
using UtauSharpApi.UTask;
using UtauSharpApi.UVoiceBank;

namespace UtauSharpApi.UPhonemizer
{
    [Phonemeizer("Auto CVVC(Presamp.ini)")]
    internal class PresampCVVCPhonemizer : IPhonemizer
    {
        internal static Dictionary<VoiceBank, Presamp.Presamp> loadCache = new Dictionary<VoiceBank, Presamp.Presamp>();
        private bool Able = true;
        private VoiceBank voiceBank;
        public PresampCVVCPhonemizer(VoiceBank vb)
        {
            voiceBank = vb;
            if (!loadCache.ContainsKey(vb)) { Init(); }
        }
        void Init()
        {
            string presamp = Path.Combine(voiceBank.vbBasePath, "presamp.ini");
            string presamp2 = Path.Combine(voiceBank.vbBasePath, "presamp.protobuf");
            if (File.Exists(presamp) || File.Exists(presamp2))
            {
                var c = Presamp.Presamp.ParsePresamp(presamp);
                if (c!=null)
                    loadCache.Add(voiceBank, c);
                else
                    Able = false;
            }
            else { Able = false; }
        }
        public bool ProcessAble()
        {
            return (!Able) ? false : loadCache.ContainsKey(voiceBank);
        }

        public List<UPhonemeNote> Process(UMidiPart MidiPart, int NoteIndex)
        {
            double Limit(double Value, double Min, double Max)
            {
                if (Value >= Max) return Max;
                if (Value <= Min) return Min;
                return Value;
            }
            List<UPhonemeNote> ret = new List<UPhonemeNote>() { new UPhonemeNote(MidiPart.Notes[NoteIndex], MidiPart.Notes[NoteIndex].Lyric) { SymbolMSec = -1 } };
            if (loadCache.ContainsKey(voiceBank))
            {
                var MCache = loadCache[voiceBank];

                UMidiNote? prevNote = NoteIndex>0?MidiPart.Notes[NoteIndex-1]:null;
                UMidiNote? curNote = MidiPart.Notes[NoteIndex];
                UMidiNote? nextNote = (NoteIndex+1)<MidiPart.Notes.Count?MidiPart.Notes[NoteIndex+1]:null;
                UMidiNote? nextNextNote = (NoteIndex + 2) < MidiPart.Notes.Count ? MidiPart.Notes[NoteIndex + 2] : null;

                Presamp.PresampSpliter sp = new Presamp.PresampSpliter(MCache, voiceBank);
                {
                    var splited = sp.SplitCVVC(prevNote, curNote, nextNote, nextNextNote);
                    if (splited.Count == 1)
                    {
                        ret.Clear();
                        ret.Add(new UPhonemeNote(curNote, splited[0].Symbol, curNote.DurationMSec));
                    }
                    else if(splited.Count>1)
                    {
                        double totalLen = curNote.DurationMSec;
                        double VCLen = splited[1].Duration;
                        double CVLen = splited[0].Duration;
                        ret.Clear();
                        ret.Add(new UPhonemeNote(curNote, splited[0].Symbol, CVLen));
                        ret.Add(new UPhonemeNote(curNote, splited[1].Symbol, VCLen));
                    }
                }
            }
            return ret;
        }
    }
}
