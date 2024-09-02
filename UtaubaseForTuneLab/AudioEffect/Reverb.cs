using NWaves.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace UtaubaseForTuneLab.AudioEffect
{
    internal class Reverb
    {
        public static float[] Mofidy(float[] data, int delayinMilliSeconds, float decayFactor, float mixPercent, int sampleRate=44100)
        {
            float[] ret = new float[data.Length];
            data.CopyTo(ret, 0);

            float[] combFilterSamples1 = CombFilter(ret, delayinMilliSeconds, decayFactor, sampleRate);
            float[] combFilterSamples2 = CombFilter(ret, delayinMilliSeconds - 11.73f, decayFactor - 0.1313f, sampleRate);
            float[] combFilterSamples3 = CombFilter(ret, delayinMilliSeconds + 19.31f, decayFactor - 0.2743f, sampleRate);
            float[] combFilterSamples4 = CombFilter(ret, delayinMilliSeconds - 7.97f, decayFactor - 0.31f, sampleRate);

            float[] comb = new float[data.Length];
            for (int i = 0; i < comb.Length; i++)
            {
                comb[i] = combFilterSamples1[i] + combFilterSamples2[i] + combFilterSamples3[i] + combFilterSamples4[i];
            }

            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = (1 - mixPercent) * ret[i] + (mixPercent * comb[i]);
            }

            float[] allPassFilter1 = AllPassFilter(ret, sampleRate);
            float[] allPassFilter2 = AllPassFilter(allPassFilter1, sampleRate);

            return allPassFilter2;
        }

        private static float[] AllPassFilter(float[] data, float sampleRate)
        {
            int delaySamples = (int)((float)89.27f * (sampleRate / 1000));
            float[] ret = new float[data.Length];
            float decayFactor = 0.131f;

            for (int i = 0; i < data.Length; i++)
            {
                float waveCurrent = data[i];
                ret[i] = waveCurrent;

                if (i - delaySamples >= 0)
                {
                    float allPassFilterSampleDelayed = ret[i - delaySamples];
                    ret[i] = ret[i] + -decayFactor * allPassFilterSampleDelayed;
                }

                if (i - delaySamples >= 1)
                {
                    float allPassFilterSampleDelayed = ret[i + 20 - delaySamples];
                    ret[i] = ret[i] + decayFactor * allPassFilterSampleDelayed;
                }
            }


            float value = ret[0];
            float max = 0.0f;

            for (int i = 0; i < ret.Length; i++)
            {
                if (Math.Abs(ret[i]) > max)
                    max = Math.Abs(ret[i]);
            }

            for (int i = 0; i < ret.Length; i++)
            {
                float currentValue = ret[i];
                value = ((value + (currentValue - value)) / max);
                ret[i] = value;
            }
            return ret;
        }

        private static float[] CombFilter(float[] data, float delayinMilliSeconds, float decayFactor, int sampleRate)
        {
            delayinMilliSeconds = Math.Max(delayinMilliSeconds, 0);
            int delaySamples = (int)(delayinMilliSeconds * (sampleRate / 1000));

            float[] combFilterSamples = new float[data.Length];
            data.CopyTo(combFilterSamples, 0);

            for (int i = 0; i < data.Length - delaySamples; i++)
            {
                float toBeDelayed = combFilterSamples[i + delaySamples];
                float delayee = combFilterSamples[i];

                combFilterSamples[i + delaySamples] = toBeDelayed + delayee * decayFactor;
            }
            return combFilterSamples;
        }

    }
}
