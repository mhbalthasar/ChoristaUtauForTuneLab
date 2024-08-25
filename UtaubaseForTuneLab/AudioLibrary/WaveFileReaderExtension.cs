using AudioLibrary.NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioLibrary.NAudio.Wave
{
    public static class WaveExtensionMethods
    {
        /// <summary>
        /// Converts a WaveProvider into a SampleProvider (only works for PCM)
        /// </summary>
        /// <param name="waveProvider">WaveProvider to convert</param>
        /// <returns></returns>
        public static ISampleProvider ToSampleProvider(this IWaveProvider waveProvider)
        {
            return SampleProviderConverters.ConvertWaveProviderIntoSampleProvider(waveProvider);
        }

        /// <summary>
        /// Converts a Stereo Sample Provider to mono, allowing mixing of channel volume
        /// </summary>
        /// <param name="sourceProvider">Stereo Source Provider</param>
        /// <param name="leftVol">Amount of left channel to mix in (0 = mute, 1 = full, 0.5 for mixing half from each channel)</param>
        /// <param name="rightVol">Amount of right channel to mix in (0 = mute, 1 = full, 0.5 for mixing half from each channel)</param>
        /// <returns>A mono SampleProvider</returns>
        public static ISampleProvider ToMono(this ISampleProvider sourceProvider, float leftVol = 0.5f, float rightVol = 0.5f)
        {
            if (sourceProvider.WaveFormat.Channels == 1) return sourceProvider;
            return new StereoToMonoSampleProvider(sourceProvider) { LeftVolume = leftVol, RightVolume = rightVol };
        }
    }
    public class StereoToMonoSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider sourceProvider;
        private float[] sourceBuffer;

        /// <summary>
        /// Creates a new mono ISampleProvider based on a stereo input
        /// </summary>
        /// <param name="sourceProvider">Stereo 16 bit PCM input</param>
        public StereoToMonoSampleProvider(ISampleProvider sourceProvider)
        {
            LeftVolume = 0.5f;
            RightVolume = 0.5f;
            if (sourceProvider.WaveFormat.Channels != 2)
            {
                throw new ArgumentException("Source must be stereo");
            }
            this.sourceProvider = sourceProvider;
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sourceProvider.WaveFormat.SampleRate, 1);
        }

        /// <summary>
        /// 1.0 to mix the mono source entirely to the left channel
        /// </summary>
        public float LeftVolume { get; set; }

        /// <summary>
        /// 1.0 to mix the mono source entirely to the right channel
        /// </summary>
        public float RightVolume { get; set; }

        /// <summary>
        /// Output Wave Format
        /// </summary>
        public WaveFormat WaveFormat { get; }

        /// <summary>
        /// Reads bytes from this SampleProvider
        /// </summary>
        public int Read(float[] buffer, int offset, int count)
        {
            var sourceSamplesRequired = count * 2;
            if (sourceBuffer == null || sourceBuffer.Length < sourceSamplesRequired) sourceBuffer = new float[sourceSamplesRequired];

            var sourceSamplesRead = sourceProvider.Read(sourceBuffer, 0, sourceSamplesRequired);
            var destOffset = offset;
            for (var sourceSample = 0; sourceSample < sourceSamplesRead; sourceSample += 2)
            {
                var left = sourceBuffer[sourceSample];
                var right = sourceBuffer[sourceSample + 1];
                var outSample = (left * LeftVolume) + (right * RightVolume);

                buffer[destOffset++] = outSample;
            }
            return sourceSamplesRead / 2;
        }
    }

    static class SampleProviderConverters
    {
        /// <summary>
        /// Helper function to go from IWaveProvider to a SampleProvider
        /// Must already be PCM or IEEE float
        /// </summary>
        /// <param name="waveProvider">The WaveProvider to convert</param>
        /// <returns>A sample provider</returns>
        public static ISampleProvider ConvertWaveProviderIntoSampleProvider(IWaveProvider waveProvider)
        {
            ISampleProvider sampleProvider;
            if (waveProvider.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
            {
                // go to float
                if (waveProvider.WaveFormat.BitsPerSample == 8)
                {
                    sampleProvider = new Pcm8BitToSampleProvider(waveProvider);
                }
                else if (waveProvider.WaveFormat.BitsPerSample == 16)
                {
                    sampleProvider = new Pcm16BitToSampleProvider(waveProvider);
                }
                else if (waveProvider.WaveFormat.BitsPerSample == 24)
                {
                    sampleProvider = new Pcm24BitToSampleProvider(waveProvider);
                }
                else if (waveProvider.WaveFormat.BitsPerSample == 32)
                {
                    sampleProvider = new Pcm32BitToSampleProvider(waveProvider);
                }
                else
                {
                    throw new InvalidOperationException("Unsupported bit depth");
                }
            }
            else if (waveProvider.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
            {
                if (waveProvider.WaveFormat.BitsPerSample == 64)
                    sampleProvider = new WaveToSampleProvider64(waveProvider);
                else
                    sampleProvider = new WaveToSampleProvider(waveProvider);
            }
            else
            {
                throw new ArgumentException("Unsupported source encoding");
            }
            return sampleProvider;
        }
    }
}
