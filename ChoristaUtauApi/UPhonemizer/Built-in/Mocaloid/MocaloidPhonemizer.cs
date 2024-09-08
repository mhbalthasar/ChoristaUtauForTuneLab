using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomlyn.Model;
using Tomlyn;
using ChoristaUtauApi.UNote;
using ChoristaUtauApi.UTask;
using ChoristaUtauApi.UVoiceBank;
using ProtoBuf.WellKnownTypes;
using ChoristaUtauApi.UPhonemizer.Presamp;

namespace ChoristaUtauApi.UPhonemizer
{
    [Phonemeizer("Auto Mocaloid")]
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
        private VoiceBank voiceBank;
        public MocaloidPhonemizer(VoiceBank vb)
        {
            voiceBank = vb;
            if (!loadCache.ContainsKey(vb)) { Init(); }
        }
        void Init()
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

                UMidiNote? curNote = MidiPart.Notes[NoteIndex];
                UMidiNote? nextNote = (NoteIndex+1)<MidiPart.Notes.Count?MidiPart.Notes[NoteIndex+1]:null;
                UMidiNote? prevNote = (NoteIndex - 1) >=0 ? MidiPart.Notes[NoteIndex - 1] : null;

                if (MCache.LangType.ToLower() == "chinese" || MCache.LangType.ToLower() == "japanese")
                {
                    string NextC = "";
                    string NextCV = "";
                    if (nextNote != null)
                    {
                        if (nextNote.Lyric == "R") { NextC = "R"; NextCV = "R"; }
                        else if (nextNote.Lyric.StartsWith(".."))
                        {
                            NextC = "";
                            NextCV = "";
                        }
                        else if (nextNote.Lyric.StartsWith("."))
                        {
                            NextC = "";
                            NextCV = "";
                            var nextL = nextNote.Lyric.Substring(1);
                            var nextA = nextL.Split(" ");
                            if (nextA.Length > 1)
                            {
                                NextC = nextA[0];
                                NextCV = nextA[0];
                            }
                            else
                            {
                                if (MCache.G2PA_Table.TryGetValue(nextL, out object gr))
                                {
                                    TomlArray pnArray = (TomlArray)gr;
                                    var gc = (string)pnArray[0];
                                    var gcv = (string)pnArray[1];
                                    var gv = (string)pnArray[2];
                                    NextC = gc;
                                    NextCV = gcv;
                                }
                            }
                        }
                        else if (MCache.G2PA_Table.TryGetValue(nextNote.Lyric, out object gr))
                        {
                            TomlArray pnArray = (TomlArray)gr;
                            var gc = (string)pnArray[0];
                            var gcv = (string)pnArray[1];
                            var gv = (string)pnArray[2];
                            NextC = gc;
                            NextCV = gcv;
                        }
                    }
                    else
                    {
                        NextC = "R";
                        NextCV = "R";
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

                        double VCLen;
                        double CVLen;
                        if (NextCV == "R")
                        {
                            CVLen = curNote.DurationMSec;
                            VCLen = Limit(CVLen * 0.2, 0, 80);
                        }
                        else if(NextCV=="")
                        {
                            CVLen = curNote.DurationMSec;
                            VCLen = 0;
                        }
                        else
                        {
                            Oto? nextCVOto = voiceBank.FindSymbol(NextCV, nextNote==null?curNote.GetNotePrefix(voiceBank):nextNote.GetNotePrefix(voiceBank));
                            VCLen = 120;
                            {

                                if (nextCVOto != null)
                                {
                                    VCLen = nextCVOto.Preutter;
                                    if (nextCVOto.Overlap == 0 && VCLen < 120) VCLen = Math.Min(120, VCLen * 2);
                                    if (nextCVOto.Overlap < 0) VCLen = (nextCVOto.Preutter - nextCVOto.Overlap);
                                }
                                {
                                    double dur = curNote.DurationMSec;
                                    var consonantStretchRatio = nextNote==null?1:Math.Pow(2, 1.0 - nextNote.Velocity * 0.01);
                                    VCLen = Convert.ToInt32(Math.Min(dur *0.2, Math.Max(60, VCLen * consonantStretchRatio)));
                                    CVLen = dur - VCLen;
                                }
;
                            }
                        }
                        //VCLen = (int)Limit(curNote.DurationMSec * 0.2, 0, 60.0);//180
                        //CVLen = curNote.DurationMSec - VCLen;
                        ret.Clear();
                        ret.Add(new UPhonemeNote(curNote, CVSymbol,CVLen));
                        if(VCLen>0)ret.Add(new UPhonemeNote(curNote, VCSymbol,VCLen));
                    }
                }
            }
            return ret;
        }
    }
}
