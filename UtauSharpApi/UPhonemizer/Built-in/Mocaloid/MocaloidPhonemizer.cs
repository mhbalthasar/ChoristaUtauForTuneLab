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
    internal class MocaloidPhonemizer : IPhonemizer
    {
        internal class MocaloidCache
        {
            public bool Loaded { get; private set; } = false;
            public string LangType { get; private set; } = "";
            public TomlTable? G2PA_Table { get; private set; } = null;
            public MocaloidCache(string TomlG2p)
            {
                string dictData = File.ReadAllText(TomlG2p);
                TomlTable? model;
                if (!Toml.TryToModel(dictData, out model, out var message)) return;
                if (model.TryGetValue("Lang", out object langObj)) { if (((TomlTable)langObj).TryGetValue("Type", out object oLangType)) { LangType = ((string)oLangType).ToLower(); }; } else return;
                if (model.TryGetValue("G2PA", out object g2paObj)) G2PA_Table = (TomlTable)g2paObj; else return;
                if (LangType == "chinese" || LangType == "japanese") Loaded = true;//Mocaloid Project Is Only Support Chinese And Japanese.Update later when the Mocaloid Update.
            }
        }
        internal static Dictionary<VoiceBank, MocaloidCache> loadCache = new Dictionary<VoiceBank, MocaloidCache>();
        private bool Able = true;
        public MocaloidPhonemizer(VoiceBank vb)
        {
            if (!loadCache.ContainsKey(vb)) { Init(vb); }
        }
        void Init(VoiceBank voiceBank)
        {
            string g2paToml = Path.Combine(voiceBank.vbBasePath, "g2pa_map.toml");
            string mocaloidIni = Path.Combine(voiceBank.vbBasePath, "mocaloid.ini");
            if (File.Exists(g2paToml) && File.Exists(mocaloidIni))
            {
                var c = new MocaloidCache(g2paToml);
                if (c.Loaded)
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

                if (MCache.LangType.ToLower() == "chinese" || MCache.LangType.ToLower() == "japanese")
                {
                    string NextC = "";
                    if (nextNote != null)
                    {
                        if (nextNote.Lyric == "R") NextC = "R";
                        else if (MCache.G2PA_Table.TryGetValue(nextNote.Lyric, out object gr))
                        {
                            TomlArray pnArray = (TomlArray)gr;
                            var gc = (string)pnArray[0];
                            var gv = (string)pnArray[2];
                            NextC = gc;
                        }
                    }
                    else
                    {
                        NextC = "R";
                    }
                    if (MCache.G2PA_Table.TryGetValue(curNote.Lyric, out object gCurN))
                    {
                        TomlArray pnArray = (TomlArray)gCurN;
                        var gc = (string)pnArray[0];
                        var gcv = (string)pnArray[1];
                        var gv = (string)pnArray[2];

                        string CVSymbol = gcv;
                        string VSymbol = gv;
                        string VCSymbol = gv + " " + NextC;

                        double totalLen = curNote.DurationMSec;
                        double VCLen = (int)Limit(totalLen * 0.2, 0, 60.0);//180
                        double CVLen = totalLen - VCLen;
                        ret.Clear();
                        ret.Add(new UPhonemeNote(curNote, CVSymbol,CVLen));
                        ret.Add(new UPhonemeNote(curNote, VCSymbol,VCLen));
                    }
                }
            }
            return ret;
        }
    }
}
