using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TuneLab.Base.Science;
using TuneLab.Extensions.Voices;
using UtauSharpApi.UNote;
using UtauSharpApi.UPhonemizer;
using UtauSharpApi.UTask;
using UtauSharpApi.UVoiceBank;

namespace UtaubaseForTuneLab.UProjectGenerator
{
    internal static class UtauProject
    {
        public static UTaskProject ProcessPhonemizer(this UTaskProject uTask,IPhonemizer PerferPhonemizer=null)
        {
            IPhonemizer phonemizer = PerferPhonemizer;
            if (phonemizer == null) phonemizer = uTask.Part.Phonemizer;
            if (phonemizer == null) phonemizer = new DefaultPhonemizer();
            for (int i = 0; i < uTask.Part.Notes.Count; i++)
            {
                uTask.Part.Notes[i].PhonemeNotes = phonemizer.ProcessEx(uTask.Part.VoiceBank, uTask.Part, i);
            }
            return uTask;
        }

        public static UTaskProject GenerateFrom(ISynthesisData data,VoiceBank vbanks)
        {
            UTaskProject ret = new UTaskProject();
            var mPart = ret.Part;
            mPart.SetVoiceBank(vbanks);
            mPart.Tempo = 125.0;//125bpm alias 1tick=1ms

            var baseStart = data.StartTime();
            var baseEnd = data.EndTime();

            var emptyTime = 2000.0;//2s Empty;

            var pNote = data.Notes.GetEnumerator();
            int count = data.Notes.Count();
            while(pNote.MoveNext()){
                var note = pNote.Current;
                if(note.Lyric =="-")
                {
                    double newEnd = (note.EndTime - baseStart) * 1000.0 + emptyTime;
                    mPart.Notes[mPart.Notes.Count - 1].DurationMSec = newEnd;//延长
                }else
                {
                    UMidiNote uNote = mPart.createNote();
                    uNote.Lyric = note.Lyric;
                    uNote.Phonemes = new List<string>();
                    uNote.StartMSec = (note.StartTime - baseStart) * 1000.0 + emptyTime;
                    uNote.DurationMSec = note.Duration() * 1000.0;
                    mPart.Notes.Add(uNote);
                }
            }
            return ret;
        }


        public static SortedDictionary<int, double> PitchPrerender(ISynthesisData data)
        {
            SortedDictionary<int, double> ret = new SortedDictionary<int, double>();

            double spt = 0;

            double preseq = 2000;
            double startTime = data.StartTime() * 1000.0;
            double endTime = data.EndTime() * 1000.0;
            if (preseq > startTime)
            {
                if (startTime < preseq)
                {
                    spt = preseq - startTime;
                    preseq -= spt;
                }
                double pt = data.Pitch.GetValue(new List<double> { 0 })[0];
                double pn = data.Notes.First().Pitch;
                if (!double.IsNaN(pt)) pn = pt;
                for (int i = 0; i < spt; i++) ret.Add(i, pn);
            }

            double startAbsTime = startTime - preseq;
            double durationAbs = endTime - startAbsTime + 1;
            double[] times = new double[(int)durationAbs];
            for (int i = 0; i < (int)durationAbs; i++) { times[i] = startAbsTime + i; }

            double[] pitchs = new double[(int)durationAbs];

            ISynthesisNote[] sNotes = data.Notes.Where(p => (
              p.StartTime < endTime && p.EndTime >= startAbsTime
            )).ToArray();

            foreach (var note in sNotes)
            {
                for (int i = 0; i < note.Duration() * 1000; i++)
                {
                    double pi = note.StartTime * 1000.0 + i;
                    if (pi >= startAbsTime && pi <= (int)endTime)
                    {
                        int idx = (int)(pi - startAbsTime);
                        pitchs[idx] = note.Pitch;
                    }
                }
            }


            double smoothTime = 120;//ms
            for (int i = 1; i < sNotes.Length; i++)
            {
                double harfSt = smoothTime / 2;
                var prevNote = sNotes[i - 1];
                var curNote = sNotes[i];
                double pst = Math.Min(prevNote.Duration() * 1000.0 / 2, harfSt);
                double cst = Math.Min(curNote.Duration() * 1000.0 / 2, harfSt);
                double p1x = prevNote.EndTime * 1000.0 - pst - startAbsTime;
                double p2x = curNote.StartTime * 1000.0 + cst - startAbsTime;
                double p1y = prevNote.Pitch;
                double p2y = curNote.Pitch;
                double gap = p2y - p1y;
                int gax = (int)(p2x - p1x);
                int midx = (int)((p2x + p1x) / 2);
                for (int t = (int)p1x; t < (int)p2x; t++)
                {
                    if (t < midx)
                    {
                        pitchs[t] = p1y + MathUtility.CubicInterpolation((t - p1x) / gax) * gap;
                    }
                    else
                    {
                        pitchs[t] = p2y - (1 - MathUtility.CubicInterpolation((t - p1x) / gax)) * gap;
                    }
                }
            }

            double[] abspit = data.Pitch.GetValue(times);
            for (int i = 0; i < abspit.Length; i++)
            {
                if (!double.IsNaN(abspit[i])) pitchs[i] = abspit[i];
                else if (double.IsNaN(pitchs[i]) || pitchs[i] == 0)
                {
                    double nearp = double.NaN;
                    for (int j = i + 1; j < pitchs.Length; j++) { if (!double.IsNaN(pitchs[j]) && pitchs[j] > 0) { nearp = pitchs[j]; break; } }
                    if (!double.IsNaN(nearp)) for (int j = i; j < pitchs.Length; j++) { if (double.IsNaN(pitchs[j]) || pitchs[j] == 0) pitchs[j] = nearp; else break; }
                }
            }

            for (int t = 0; t < pitchs.Length; t++)
            {
                int Key = (int)(t + spt);
                if (ret.ContainsKey(Key)) ret[Key] = pitchs[t];
                else ret.Add(Key, pitchs[t]);
            }
            return ret;
        }


        public static List<string> GenerateWavToolArgs(URenderNote rNote,string OutputWavFile)
        {
            return rNote.GetWavToolArgs(OutputWavFile);
        }
        public static List<string> GenerateResamplerArgs(URenderNote rNote, string OutputWavFile)
        {
            return rNote.GetResamplerArgs();
        }
    }   
}
