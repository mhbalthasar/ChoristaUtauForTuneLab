using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtaubaseForTuneLab.AudioEffect
{
    internal class Compressor
    {
        private class Decibels
        {
            // 20 / ln( 10 )
            private const double LOG_2_DB = 8.6858896380650365530225783783321;

            // ln( 10 ) / 20
            private const double DB_2_LOG = 0.11512925464970228420089957273422;

            /// <summary>
            /// linear to dB conversion
            /// </summary>
            /// <param name="lin">linear value</param>
            /// <returns>decibel value</returns>
            public static double LinearToDecibels(double lin)
            {
                return Math.Log(lin) * LOG_2_DB;
            }

            /// <summary>
            /// dB to linear conversion
            /// </summary>
            /// <param name="dB">decibel value</param>
            /// <returns>linear value</returns>
            public static double DecibelsToLinear(double dB)
            {
                return Math.Exp(dB * DB_2_LOG);
            }

        }
        private class EnvelopeDetector
        {
            private double sampleRate;
            private double ms;
            private double coeff;

            public EnvelopeDetector() : this(1.0, 44100.0)
            {
            }

            public EnvelopeDetector(double ms, double sampleRate)
            {
                System.Diagnostics.Debug.Assert(sampleRate > 0.0);
                System.Diagnostics.Debug.Assert(ms > 0.0);
                this.sampleRate = sampleRate;
                this.ms = ms;
                SetCoef();
            }

            public double TimeConstant
            {
                get => ms;
                set
                {
                    System.Diagnostics.Debug.Assert(value > 0.0);
                    this.ms = value;
                    SetCoef();
                }
            }

            public double SampleRate
            {
                get => sampleRate;
                set
                {
                    System.Diagnostics.Debug.Assert(value > 0.0);
                    this.sampleRate = value;
                    SetCoef();
                }
            }

            public double Run(double inValue, double state)
            {
                return inValue + coeff * (state - inValue);
            }

            private void SetCoef()
            {
                coeff = Math.Exp(-1.0 / (0.001 * ms * sampleRate));
            }
        }
        public static float[] Mofidy(float[] data, double Threshold=16, double Ratio=6,double MakeUpGain=0, double AttrackTime=10.0,double ReleaseTime=10.0,int sampleRate = 44100)
        {
            const double DC_OFFSET = 1.0E-25;
            double envdB = 0;

            EnvelopeDetector attack = new EnvelopeDetector(AttrackTime, sampleRate);
            EnvelopeDetector release = new EnvelopeDetector(ReleaseTime, sampleRate);

            for (int i = 0; i < data.Length; i++)
            {
                // sidechain

                // rectify input
                double rect1 = Math.Abs(data[i]); // n.b. was fabs

                // if desired, one could use another EnvelopeDetector to smooth
                // the rectified signal.

                double link = rect1;   // link channels with greater of 2

                link += DC_OFFSET; // add DC offset to avoid log( 0 )
                double keydB = Decibels.LinearToDecibels(link); // convert linear -> dB

                // threshold
                double overdB = keydB - Threshold; // delta over threshold
                if (overdB < 0.0)
                    overdB = 0.0;

                // attack/release

                overdB += DC_OFFSET; // add DC offset to avoid denormal

                envdB = overdB > envdB ? attack.Run(overdB, envdB) : release.Run(overdB, envdB); // run attack/release envelope

                overdB = envdB - DC_OFFSET; // subtract DC offset

                // Regarding the DC offset: In this case, since the offset is added before 
                // the attack/release processes, the envelope will never fall below the offset,
                // thereby avoiding denormals. However, to prevent the offset from causing
                // constant gain reduction, we must subtract it from the envelope, yielding
                // a minimum value of 0dB.

                // transfer function
                double gr = overdB * (Ratio - 1.0); // gain reduction (dB)
                gr = Decibels.DecibelsToLinear(gr);
                double ga = Decibels.DecibelsToLinear(MakeUpGain); // convert dB -> linear
                
                // output gain
                data[i] = (float)(data[i]*ga*gr);  // apply gain reduction to input
            }
            return data;
        }
    }
}
