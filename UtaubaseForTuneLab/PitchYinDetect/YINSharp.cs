using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UtaubaseForTuneLab
{
    public class YINSharp
    {
        public static double PnFromYin(float[] samples,
                                    int samplingRate,
                                    int startPos = 0,
                                    int endPos = -1,
                                    float low = 80/*Hz*/,
                                    float high = 400/*Hz*/,
                                    float cmdfThreshold = 0.2f)
        {
            float frequency = FromYin(samples, samplingRate, startPos, endPos, low, high, cmdfThreshold);
            double ratio = frequency / 440.0;
            double log2Ratio = Math.Log(ratio, 2);
            return 69 + 12 * log2Ratio;
        }
        /// <summary>
        /// <para>Estimates pitch from <paramref name="samples"/> using YIN algorithm:</para>
        /// <para>
        /// De Cheveigne, A., Kawahara, H. YIN, a fundamental frequency estimator for speech and music. 
        /// The Journal of the Acoustical Society of America, 111(4). - 2002.
        /// </para>
        /// </summary>
        /// <param name="samples">Array of samples</param>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="startPos">Index of the first sample in array for processing</param>
        /// <param name="endPos">Index of the last sample in array for processing</param>
        /// <param name="low">Lower frequency of expected pitch range</param>
        /// <param name="high">Upper frequency of expected pitch range</param>
        /// <param name="cmdfThreshold">CMDF threshold</param>
        public static float FromYin(float[] samples,
                                    int samplingRate,
                                    int startPos = 0,
                                    int endPos = -1,
                                    float low = 80/*Hz*/,
                                    float high = 400/*Hz*/,
                                    float cmdfThreshold = 0.2f)
        {
            if (endPos == -1)
            {
                endPos = samples.Length;
            }

            var pitch1 = (int)(1.0 * samplingRate / high);    // 2,5 ms = 400Hz
            var pitch2 = (int)(1.0 * samplingRate / low);     // 12,5 ms = 80Hz
            
            var length = (endPos - startPos) / 2;

            // cumulative mean difference function (CMDF):

            var cmdf = new float[length];

            for (var i = 0; i < length; i++)
            {
                for (var j = 0; j < length; j++)
                {
                    var diff = samples[j + startPos] - samples[i + j + startPos];
                    cmdf[i] += diff * diff;
                }
            }

            cmdf[0] = 1;

            var sum = 0.0f;
            for (var i = 1; i < length; i++)
            {
                sum += cmdf[i];
                cmdf[i] *= i / sum;
            }

            // adjust t according to some threshold:

            var t = pitch1;     // focusing on range [pitch1 .. pitch2]

            for (; t < pitch2; t++)
            {
                if (cmdf[t] < cmdfThreshold)
                {
                    while (t + 1 < pitch2 && cmdf[t + 1] < cmdf[t]) t++;
                    break;
                }
            }

            // no pitch

            if (t == pitch2 || cmdf[t] >= cmdfThreshold)
            {
                return 0.0f;
            }

            // parabolic interpolation:

            var x1 = t < 1 ? t : t - 1;
            var x2 = t + 1 < length ? t + 1 : t;

            if (t == x1)
            {
                if (cmdf[t] > cmdf[x2])
                {
                    t = x2;
                }
            }
            else if (t == x2)
            {
                if (cmdf[t] > cmdf[x1])
                {
                    t = x1;
                }
            }
            else
            {
                t = (int)(t + (cmdf[x2] - cmdf[x1]) / (2 * cmdf[t] - cmdf[x2] - cmdf[x1]) / 2);
            }

            return samplingRate / t;
        }
    }
}
