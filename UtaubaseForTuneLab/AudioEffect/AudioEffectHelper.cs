using NWaves.Effects;
using NWaves.Filters.Base;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuneLab.Base.Properties;
using TuneLab.Base.Structures;
using TuneLab.Extensions.Voices;
using UtaubaseForTuneLab.UProjectGenerator;

namespace UtaubaseForTuneLab.AudioEffect
{
    internal class AudioEffectHelper
    {
        public const string Afx_Compressor_thresholdDb_ID = "AFX:Compressor thresholdDb";
        public readonly static NumberConfig Afx_Compressor_thresholdDb_Config = new() { DefaultValue = 24.0, MinValue = -24, MaxValue = 24, IsInterger = true };
        public const string Afx_Compressor_ratio_ID = "AFX:Compressor ratio";
        public readonly static NumberConfig Afx_Compressor_ratio_Config = new() { DefaultValue = 0, MinValue = -30, MaxValue = 30 };
        public const string Afx_Compressor_BoosterDb_ID = "AFX:Compressor BoosterDb";
        public readonly static NumberConfig Afx_Compressor_BoosterDb_Config = new() { DefaultValue = 0.0, MinValue = -24, MaxValue = 24, IsInterger = true };
        public const string Afx_Compressor_Att_ID = "AFX:Compressor AttrackTime";
        public readonly static NumberConfig Afx_Compressor_Att_Config = new() { DefaultValue = 10.0, MinValue = 1, MaxValue = 100, IsInterger = true };
        public const string Afx_Compressor_Rlt_ID = "AFX:Compressor ReleaseTime";
        public readonly static NumberConfig Afx_Compressor_Rlt_Config = new() { DefaultValue = 10.0, MinValue = 1, MaxValue = 100, IsInterger = true };

        public const string Afx_Reverb_Mix_ID = "AFX:Reverb Mix";
        public readonly static NumberConfig Afx_Reverb_Mix_Config = new() { DefaultValue = 0.0, MinValue = 0, MaxValue = 1 };
        public const string Afx_Reverb_Delay_ID = "AFX:Reverb Delay";
        public readonly static NumberConfig Afx_Reverb_Delay_Config = new() { DefaultValue = 0.2, MinValue = 0, MaxValue = 5 };
        public const string Afx_Reverb_Decay_ID = "AFX:Reverb DecayFactor";
        public readonly static NumberConfig Afx_Reverb_Decay_Config = new() { DefaultValue = 0.3, MinValue = 0, MaxValue = 1 };

        public const string Afx_Eq_Bank_60Hz_ID = "AFX:EQ 60Hz";
        public const string Afx_Eq_Bank_250Hz_ID = "AFX:EQ 250Hz";
        public const string Afx_Eq_Bank_1kHz_ID = "AFX:EQ 1kHz";
        public const string Afx_Eq_Bank_4kHz_ID = "AFX:EQ 4kHz";
        public const string Afx_Eq_Bank_16kHz_ID = "AFX:EQ 16kHz";
        public readonly static NumberConfig Afx_EqBand_Config = new() { DefaultValue = 0, MinValue = -20, MaxValue = 20 };

        public static OrderedMap<string, IPropertyConfig> PartProperties = new OrderedMap<string, IPropertyConfig>()
        {
                    {Afx_Compressor_thresholdDb_ID,Afx_Compressor_thresholdDb_Config },
                    {Afx_Compressor_ratio_ID,Afx_Compressor_ratio_Config },
                    {Afx_Compressor_BoosterDb_ID,Afx_Compressor_BoosterDb_Config },
                    {Afx_Compressor_Att_ID,Afx_Compressor_Att_Config },
                    {Afx_Compressor_Rlt_ID,Afx_Compressor_Rlt_Config },

                    {Afx_Eq_Bank_60Hz_ID,Afx_EqBand_Config },
                    {Afx_Eq_Bank_250Hz_ID,Afx_EqBand_Config },
                    {Afx_Eq_Bank_1kHz_ID,Afx_EqBand_Config },
                    {Afx_Eq_Bank_4kHz_ID,Afx_EqBand_Config },
                    {Afx_Eq_Bank_16kHz_ID,Afx_EqBand_Config },

                    {Afx_Reverb_Mix_ID,Afx_Reverb_Mix_Config},
                    {Afx_Reverb_Delay_ID,Afx_Reverb_Delay_Config},
                    {Afx_Reverb_Decay_ID,Afx_Reverb_Decay_Config}
        };
        public static void DoProcess(ISynthesisData pData,ref TaskAudioData audioData)
        {
            //Compressor
            {
                int thresholdDb = pData.PartProperties.GetInt(Afx_Compressor_thresholdDb_ID, 0);
                int boosterDb = pData.PartProperties.GetInt(Afx_Compressor_BoosterDb_ID, 0);
                double ratio = pData.PartProperties.GetDouble(Afx_Compressor_ratio_ID, 1);
                int att = pData.PartProperties.GetInt(Afx_Compressor_Att_ID, 10);
                int rlt = pData.PartProperties.GetInt(Afx_Compressor_Rlt_ID, 10);
                if (thresholdDb < 24 && ratio != 0) audioData.audio_Data = Compressor.Mofidy(audioData.audio_Data, thresholdDb, ratio, boosterDb, att, rlt, (int)audioData.audio_SampleRate);
            }
            //Equalizer
            {
                double eb1 = pData.PartProperties.GetDouble(Afx_Eq_Bank_60Hz_ID, 0.0);
                double eb2 = pData.PartProperties.GetDouble(Afx_Eq_Bank_250Hz_ID, 0.0);
                double eb3 = pData.PartProperties.GetDouble(Afx_Eq_Bank_1kHz_ID, 0.0);
                double eb4 = pData.PartProperties.GetDouble(Afx_Eq_Bank_4kHz_ID, 0.0);
                double eb5 = pData.PartProperties.GetDouble(Afx_Eq_Bank_16kHz_ID, 0.0);
                if(eb1!=0 || eb2!=0 || eb3!=0 || eb4!=0 || eb5!=0)
                    audioData.audio_Data = Equalizer.Mofidy(
                        audioData.audio_Data,
                        eb1,
                        eb2,
                        eb3,
                        eb4,
                        eb5,
                        (int)audioData.audio_SampleRate
                        );
            }

            //Reverb
            {
                double mix = pData.PartProperties.GetDouble(Afx_Reverb_Mix_ID, 0.0);
                double delay = pData.PartProperties.GetDouble(Afx_Reverb_Delay_ID, 0.2);
                double decay = pData.PartProperties.GetDouble(Afx_Reverb_Decay_ID, 0.3);
                if (mix > 0) audioData.audio_Data = Reverb.Mofidy(audioData.audio_Data, (int)Math.Round(delay * 100.0), (float)decay, (float)mix, (int)audioData.audio_SampleRate);
            }
        }
        public static TaskAudioData MixSynthesis(ISynthesisData synthesisData, TaskAudioData audioInfo, TaskAudioData xAudioInfo)
        {
            double startTime = audioInfo.audio_StartMillsec;
            double endTime = audioInfo.audio_StartMillsec+audioInfo.audio_DurationMillsec;
            int sampleIndex=0;
            List<double> times = new List<double>();
            while(times.LastOrDefault(0)<=(endTime/1000.0))
            {
                double time = (startTime / 1000.0)+sampleIndex/audioInfo.audio_SampleRate;
                sampleIndex++;
                times.Add(time);
            }

            int delayXSample = (int)((xAudioInfo.audio_StartMillsec - startTime) / 1000.0 * audioInfo.audio_SampleRate);

            synthesisData.GetAutomation(UtauEngine.XTrack_XPrefixKeyID, out var automation);
            if (automation != null)
            {
                try
                {
                    double[] values = automation.GetValue(times);
                    //xTrack is after origin
                    for (int i = Math.Max(0, delayXSample); i < audioInfo.audio_Data.Length; i++)
                    {
                        int j = i - delayXSample;
                        if (j >= xAudioInfo.audio_Data.Length) break;
                        float t1p = (float)(1.0 - values[i]);
                        float t2p = (float)(values[i]);
                        audioInfo.audio_Data[i] = audioInfo.audio_Data[i] * t1p + xAudioInfo.audio_Data[j] * t2p;
                    }
                }
                catch {; }
            }

            return audioInfo;
        }
    }
}
