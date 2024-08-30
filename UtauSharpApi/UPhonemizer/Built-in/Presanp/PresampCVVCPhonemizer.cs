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
    internal class PresampCVVCPhonemizer : IPhonemizer
    {
        internal static Dictionary<VoiceBank, Presamp.Presamp> loadCache = new Dictionary<VoiceBank, Presamp.Presamp>();
        private bool Able = true;
        public PresampCVVCPhonemizer(VoiceBank vb)
        {
            if (!loadCache.ContainsKey(vb)) { Init(vb); }
        }
        void Init(VoiceBank voiceBank)
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
        public List<string> Process(UMidiPart MidiPart, int NoteIndex)
        {
            throw new NotImplementedException();
        }

        public bool ProcessAble(VoiceBank voiceBank)
        {
            return (!Able) ? false : loadCache.ContainsKey(voiceBank);
        }

        public List<UPhonemeNote> ProcessEx(VoiceBank voiceBank, UMidiPart MidiPart, int NoteIndex)
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

                UMidiNote? curNote = MidiPart.Notes[NoteIndex];
                UMidiNote? nextNote = (NoteIndex+1)<MidiPart.Notes.Count?MidiPart.Notes[NoteIndex+1]:null;
                UMidiNote? prevNote = (NoteIndex - 1) >=0 ? MidiPart.Notes[NoteIndex - 1] : null;

                Presamp.PresampSpliter sp = new Presamp.PresampSpliter(MCache, voiceBank);
                {
                    Presamp.PresampSpliter.PresampNote inputNote = new Presamp.PresampSpliter.PresampNote(curNote);
                    Presamp.PresampSpliter.PresampNote? inputNextNote = nextNote == null ? null : new Presamp.PresampSpliter.PresampNote(nextNote);
                    var splited = sp.SplitCVVC(inputNote, inputNextNote, curNote.NoteNumber);
                    if (splited.Count == 1)
                    {
                        ret.Clear();
                        ret.Add(new UPhonemeNote(curNote, splited[0].Symbol, curNote.DurationMSec));
                    }
                    else if(splited.Count>1)
                    {
                        double totalLen = curNote.DurationMSec;
                        double VCLen = (int)Limit(totalLen * 0.2, 0, 60.0);
                        double CVLen = totalLen - VCLen;
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
