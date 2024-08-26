using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtauSharpApi.UNote;
using UtauSharpApi.UPhonemizer;
using UtauSharpApi.UVoiceBank;

namespace UtauSharpApi.UTask
{
    public class UMidiPart
    {
        public List<UMidiNote> Notes { get; set; } = new List<UMidiNote>();
        public IPhonemizer Phonemizer { get; set; } = new DefaultPhonemizer();
        public VoiceBank? VoiceBank { get; private set; } = null;

        public void SetVoiceBank(VoiceBank vb)
        {
            VoiceBank = vb;
            Phonemizer=PhonemizerSelector.GuessPhonemizer(vb);
        }

        public UMidiNote createNote()
        {
            return new UMidiNote(this);
        }

        public List<URenderNote> GenerateRendPart(string EngineSalt="")
        {
            double curMs = 0;
            double minR = 0;
            List<URenderNote> ret = new List<URenderNote>();
            foreach(var note in Notes)
            {
                if (note.StartMSec - curMs > minR)
                {
                    URenderNote rNote = new URenderNote();
                    rNote.RenderOto = Oto.GetR;
                    rNote.VoiceBankPath = "";
                    rNote.StartMSec = curMs;
                    rNote.DurationMSec = note.StartMSec - curMs;
                    curMs = note.StartMSec;
                    if (ret.Count > 0)
                    {
                        rNote.PrevNote = ret[ret.Count - 1];
                        ret[ret.Count - 1].NextNote = rNote;
                    }
                    rNote.FixSTP.UpdateSTPFix();
                    ret.Add(rNote);
                }
                else curMs = note.StartMSec;
                double staticLength = 0;
                double dynmaticPartCount = 0;
                foreach (UPhonemeNote ppNote in note.PhonemeNotes)
                {
                    if (ppNote.SymbolMSec > 0) staticLength += ppNote.SymbolMSec;
                    else dynmaticPartCount=dynmaticPartCount+1;
                }
                double dynmaticLength = (note.DurationMSec - staticLength) / dynmaticPartCount;
                foreach (UPhonemeNote ppNote in note.PhonemeNotes)
                {
                    URenderNote rpNote = new URenderNote();
                    rpNote.RenderOto = VoiceBank.FindSymbol(ppNote.Symbol,note.NoteNumber);
                    rpNote.VoiceBankPath = VoiceBank.vbBasePath;
                    rpNote.NoteNumber = note.NoteNumber;
                    rpNote.Velocity = note.Velocity;
                    rpNote.Flags = note.Flags;
                    rpNote.EngineSalt = EngineSalt;
                    rpNote.StartMSec = curMs;
                    rpNote.DurationMSec = (ppNote.SymbolMSec<=0)?dynmaticLength:ppNote.SymbolMSec;
                    curMs = curMs+ rpNote.DurationMSec;
                    if (ret.Count > 0)
                    {
                        rpNote.PrevNote = ret[ret.Count - 1];
                        ret[ret.Count - 1].NextNote = rpNote;
                    }
                    rpNote.FixSTP.UpdateSTPFix();
                    ret.Add(rpNote);
                }
            }
            return ret;
        }

        public string GetBatchBat()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("@rem project=TunelabRender");
            sb.AppendLine("@set loadmodule=");
            sb.AppendLine("@set tempo=125");//MAX TEMPO:500,1tick=0.2ms
            sb.AppendLine("@set samples=44100");
            sb.AppendLine("@set oto="+VoiceBank.vbBasePath);
            sb.AppendLine("@set flag=\"\"");
            sb.AppendLine("@set env=0 5 35 0 100 100 0");
            sb.AppendLine("@set stp=0");

            return sb.ToString();
        }

    }
}
