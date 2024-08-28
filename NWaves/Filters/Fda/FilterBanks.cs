﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NWaves.Filters.Base;
using NWaves.Filters.BiQuad;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;

namespace NWaves.Filters.Fda
{
    /// <summary>
    /// Contains methods providing general shapes of filter banks:
    /// <list type="bullet">
    ///     <item>triangular</item>
    ///     <item>rectangular</item>
    ///     <item>FIR bandpass (close to trapezoidal, slightly overlapping)</item>
    ///     <item>BiQuad bandpass</item>
    /// </list>
    /// 
    /// ...and methods for obtaining the most widely used frequency bands:
    /// 
    /// <list type="bullet">
    ///     <item>Herz bands</item>
    ///     <item>Mel bands (HTK and Slaney)</item>
    ///     <item>Bark bands (uniform and Slaney)</item>
    ///     <item>Critical bands</item>
    ///     <item>ERB filterbank</item>
    ///     <item>Octaves (from MPEG-7)</item>
    ///     <item>Chroma</item>
    /// </list>
    /// </summary>
    public static class FilterBanks
    {
        /// <summary>
        /// Generates triangular filterbank weights based on given <paramref name="frequencies"/>.
        /// </summary>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="frequencies">Array of frequency tuples (left, center, right) for each filter</param>
        /// <param name="vtln">VTLN frequency warper</param>
        /// <param name="mapper">Frequency scale mapper (e.g. herz-to-mel) used here only for proper weighting</param>
        public static float[][] Triangular(int fftSize,
                                           int samplingRate,
                                           (double, double, double)[] frequencies,
                                           VtlnWarper vtln = null,
                                           Func<double, double> mapper = null)
        {
            if (mapper is null) mapper = x => x;

            Func<double, double> warp = vtln is null ? mapper : x => mapper(vtln.Warp(x));

            var herzResolution = (double)samplingRate / fftSize;

            var herzFrequencies = Enumerable.Range(0, fftSize / 2 + 1)
                                            .Select(f => f * herzResolution)
                                            .ToArray();

            var filterCount = frequencies.Length;
            var filterBank = new float[filterCount][];

            for (var i = 0; i < filterCount; i++)
            {
                filterBank[i] = new float[fftSize / 2 + 1];

                var (left, center, right) = frequencies[i];

                left = warp(left);
                center = warp(center);
                right = warp(right);

                var j = 0;
                for (; mapper(herzFrequencies[j]) <= left; j++) ;
                for (; mapper(herzFrequencies[j]) <= center; j++)
                {
                    filterBank[i][j] = (float)((mapper(herzFrequencies[j]) - left) / (center - left));
                }
                for (; j < herzFrequencies.Length && mapper(herzFrequencies[j]) < right; j++)
                {
                    filterBank[i][j] = (float)((right - mapper(herzFrequencies[j])) / (right - center));
                }
            }

            return filterBank;
        }

        /// <summary>
        /// Generates rectangular filterbank weights based on given <paramref name="frequencies"/>.
        /// </summary>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="frequencies">Array of frequency tuples (left, center, right) for each filter</param>
        /// <param name="vtln">VTLN frequency warper</param>
        /// <param name="mapper">Frequency scale mapper (e.g. herz-to-mel)</param>
        public static float[][] Rectangular(int fftSize,
                                           int samplingRate,
                                           (double, double, double)[] frequencies,
                                           VtlnWarper vtln = null,
                                           Func<double, double> mapper = null)
        {
            if (mapper is null) mapper = x => x;

            Func<double, double> warp = vtln is null ? mapper : x => mapper(vtln.Warp(x));

            var herzResolution = (double)samplingRate / fftSize;

            var herzFrequencies = Enumerable.Range(0, fftSize / 2 + 1)
                                            .Select(f => f * herzResolution)
                                            .ToArray();

            var filterCount = frequencies.Length;
            var filterBank = new float[filterCount][];

            for (var i = 0; i < filterCount; i++)
            {
                filterBank[i] = new float[fftSize / 2 + 1];

                var (left, center, right) = frequencies[i];

                left = warp(left);
                center = warp(center);
                right = warp(right);

                var j = 0;
                for (; mapper(herzFrequencies[j]) <= left; j++) ;
                for (; j < herzFrequencies.Length && mapper(herzFrequencies[j]) < right; j++)
                {
                    filterBank[i][j] = 1;
                }
            }

            return filterBank;
        }

        /// <summary>
        /// Generates FIR bandpass (close to trapezoidal) filterbank based on given <paramref name="frequencies"/>.
        /// </summary>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="frequencies">Array of frequency tuples (left, center, right) for each filter</param>
        /// <param name="vtln">VTLN frequency warper</param>
        /// <param name="mapper">Frequency scale mapper (e.g. herz-to-mel)</param>
        public static float[][] Trapezoidal(int fftSize,
                                           int samplingRate,
                                           (double, double, double)[] frequencies,
                                           VtlnWarper vtln = null,
                                           Func<double, double> mapper = null)
        {
            var filterBank = Rectangular(fftSize, samplingRate, frequencies, vtln, mapper);

            for (var i = 0; i < filterBank.Length; i++)
            {
                var order = fftSize / 4 + 1;
                var kernel = DesignFilter.Fir(order, null, filterBank[i].ToDoubles(), fftSize);

                var filterTf = new TransferFunction(kernel);

                filterBank[i] = filterTf.FrequencyResponse(fftSize).Magnitude.ToFloats();

                // normalize gain to 1.0

                var maxAmp = 0.0f;
                for (var j = 0; j < filterBank[i].Length; j++)
                {
                    if (filterBank[i][j] > maxAmp) maxAmp = filterBank[i][j];
                }
                for (var j = 0; j < filterBank[i].Length; j++)
                {
                    filterBank[i][j] /= maxAmp;
                }
            }

            return filterBank;
        }

        /// <summary>
        /// Generates BiQuad bandpass overlapping filters based on given <paramref name="frequencies"/>.
        /// </summary>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="frequencies">Array of frequency tuples (left, center, right) for each filter</param>
        public static float[][] BiQuad(int fftSize, int samplingRate, (double, double, double)[] frequencies)
        {
            var center = frequencies.Select(f => f.Item2).ToArray();

            var filterCount = frequencies.Length;
            var filterBank = new float[filterCount][];

            for (var i = 0; i < filterCount; i++)
            {
                var freq = center[i] / samplingRate;
                var filter = new BandPassFilter(freq, 2.0);

                filterBank[i] = filter.Tf.FrequencyResponse(fftSize).Magnitude.ToFloats();
            }

            return filterBank;
        }

        /// <summary>
        /// Returns frequency tuples for uniformly spaced frequency bands on any scale.
        /// </summary>
        /// <param name="scaleMapper">The function that converts Hz to other frequency scale</param>
        /// <param name="inverseMapper">The function that converts frequency from alternate scale back to Hz</param>
        /// <param name="filterCount">Number of filters</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <param name="overlap">Flag indicating that bands should overlap</param>
        private static (double, double, double)[] UniformBands(
                                                     Func<double, double> scaleMapper,
                                                     Func<double, double> inverseMapper,
                                                     int filterCount, 
                                                     int samplingRate, 
                                                     double lowFreq = 0,
                                                     double highFreq = 0, 
                                                     bool overlap = true)
        {
            if (lowFreq < 0)
            {
                lowFreq = 0;
            }
            if (highFreq <= lowFreq)
            {
                highFreq = samplingRate / 2.0;
            }

            var startingFrequency = scaleMapper(lowFreq);

            var frequencyTuples = new (double, double, double)[filterCount];

            if (overlap)
            {
                var newResolution = (scaleMapper(highFreq) - scaleMapper(lowFreq)) / (filterCount + 1);

                var frequencies = Enumerable.Range(0, filterCount + 2)
                                            .Select(i => inverseMapper(startingFrequency + i * newResolution))
                                            .ToArray();
                
                for (var i = 0; i < filterCount; i++)
                {
                    frequencyTuples[i] = (frequencies[i], frequencies[i + 1], frequencies[i + 2]);
                }
            }
            else
            {
                var newResolution = (scaleMapper(highFreq) - scaleMapper(lowFreq)) / filterCount;

                var frequencies = Enumerable.Range(0, filterCount + 1)
                                            .Select(i => inverseMapper(startingFrequency + i * newResolution))
                                            .ToArray();
                
                for (var i = 0; i < filterCount; i++)
                {
                    frequencyTuples[i] = (frequencies[i], (frequencies[i] + frequencies[i + 1]) / 2, frequencies[i + 1]);
                }
            }

            return frequencyTuples;
        }

        /// <summary>
        /// Returns frequency tuples for uniformly spaced frequency bands on Herz scale.
        /// </summary>
        /// <param name="combFilterCount">Number of filters</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <param name="overlap">Flag indicating that bands should overlap</param>
        public static (double, double, double)[] HerzBands(
            int combFilterCount, int samplingRate, double lowFreq = 0, double highFreq = 0, bool overlap = false)
        {
            // "x => x" means map frequency 1-to-1 (in Hz as it is)
            return UniformBands(x => x, x => x, combFilterCount, samplingRate, lowFreq, highFreq, overlap);
        }

        /// <summary>
        /// Returns frequency tuples for uniformly spaced frequency bands on Mel scale.
        /// </summary>
        /// <param name="melFilterCount">Number of mel filters to create</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <param name="overlap">Flag indicating that bands should overlap</param>
        public static (double, double, double)[] MelBands(
            int melFilterCount, int samplingRate, double lowFreq = 0, double highFreq = 0, bool overlap = true)
        {
            return UniformBands(Scale.HerzToMel, Scale.MelToHerz, melFilterCount, samplingRate, lowFreq, highFreq, overlap);
        }

        /// <summary>
        /// Returns frequency tuples for uniformly spaced frequency bands on Mel scale 
        /// (according to M.Slaney's formula).
        /// </summary>
        /// <param name="melFilterCount">Number of mel filters to create</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <param name="overlap">Flag indicating that bands should overlap</param>
        public static (double, double, double)[] MelBandsSlaney(
            int melFilterCount, int samplingRate, double lowFreq = 0, double highFreq = 0, bool overlap = true)
        {
            return UniformBands(Scale.HerzToMelSlaney, Scale.MelToHerzSlaney, melFilterCount, samplingRate, lowFreq, highFreq, overlap);
        }

        /// <summary>
        /// Returns frequency tuples for uniformly spaced frequency bands on Bark scale (Traunmueller, 1990).
        /// </summary>
        /// <param name="barkFilterCount">Number of bark filters to create</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <param name="overlap">Flag indicating that bands should overlap</param>
        public static (double, double, double)[] BarkBands(
            int barkFilterCount, int samplingRate, double lowFreq = 0, double highFreq = 0, bool overlap = true)
        {
            return UniformBands(Scale.HerzToBark, Scale.BarkToHerz, barkFilterCount, samplingRate, lowFreq, highFreq, overlap);
        }

        /// <summary>
        /// Returns frequency tuples for uniformly spaced frequency bands on Bark scale (Wang, 1992).
        /// </summary>
        /// <param name="barkFilterCount">Number of bark filters to create</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <param name="overlap">Flag indicating that bands should overlap</param>
        public static (double, double, double)[] BarkBandsSlaney(
            int barkFilterCount, int samplingRate, double lowFreq = 0, double highFreq = 0, bool overlap = true)
        {
            return UniformBands(Scale.HerzToBarkSlaney, Scale.BarkToHerzSlaney, barkFilterCount, samplingRate, lowFreq, highFreq, overlap);
        }

        /// <summary>
        /// Returns frequency tuples for critical bands.
        /// </summary>
        /// <param name="filterCount">Number of filters to create</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        public static (double, double, double)[] CriticalBands(
            int filterCount, int samplingRate, double lowFreq = 0, double highFreq = 0)
        {
            if (lowFreq < 0)
            {
                lowFreq = 0;
            }
            if (highFreq <= lowFreq)
            {
                highFreq = samplingRate / 2.0;
            }

            double[] edgeFrequencies = { 20,   100,  200,  300,  400,  510,  630,  770,  920,  1080, 1270,  1480,  1720,
                                         2000, 2320, 2700, 3150, 3700, 4400, 5300, 6400, 7700, 9500, 12000, 15500, 20500 };

            double[] centerFrequencies = { 50,   150,  250,  350,  450,  570,  700,  840,  1000, 1170, 1370,  1600,
                                           1850, 2150, 2500, 2900, 3400, 4000, 4800, 5800, 7000, 8500, 10500, 13500, 17500 };

            var startIndex = 0;
            for (var i = 0; i < centerFrequencies.Length; i++)
            {
                if (centerFrequencies[i] < lowFreq) continue;
                startIndex = i;
                break;
            }

            var endIndex = 0;
            for (var i = centerFrequencies.Length - 1; i >= 0; i--)
            {
                if (centerFrequencies[i] > highFreq) continue;
                endIndex = i;
                break;
            }

            filterCount = Math.Min(endIndex - startIndex + 1, filterCount);

            var edges = edgeFrequencies.Skip(startIndex)
                                       .Take(filterCount + 1)
                                       .ToArray();

            var centers = centerFrequencies.Skip(startIndex)
                                           .Take(filterCount)
                                           .ToArray();

            var frequencyTuples = new (double, double, double)[filterCount];

            for (var i = 0; i < filterCount; i++)
            {
                frequencyTuples[i] = (edges[i], centers[i], edges[i + 1]);
            }

            return frequencyTuples;
        }

        /// <summary>
        /// Returns frequency tuples for octave bands.
        /// </summary>
        /// <param name="octaveCount">Number of octave filters to create</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <param name="overlap">Flag indicating that bands should overlap</param>
        public static (double, double, double)[] OctaveBands(
            int octaveCount, int samplingRate, double lowFreq = 0, double highFreq = 0, bool overlap = false)
        {
            if (lowFreq < 1e-10)
            {
                lowFreq = 62.5;//Hz
            }

            if (highFreq <= lowFreq)
            {
                highFreq = samplingRate / 2.0;
            }

            var f1 = lowFreq;
            var f2 = lowFreq * 2;

            var frequencyTuples = new List<(double, double, double)>();

            if (overlap)
            {
                var f3 = f2 * 2;

                for (var i = 0; i < octaveCount && f3 < highFreq; i++)
                {
                    frequencyTuples.Add((f1, f2, f3));
                    f1 = f2;
                    f2 = f3;
                    f3 *= 2;
                }
            }
            else
            {
                for (var i = 0; i < octaveCount && f2 < highFreq; i++)
                {
                    frequencyTuples.Add((f1, (f1 + f2) / 2, f2));
                    f1 *= 2;
                    f2 *= 2;
                }
            }

            return frequencyTuples.ToArray();
        }

        /// <summary>
        /// Generates chroma feature filter bank (slightly over-complicated to match librosa analog).
        /// </summary>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate</param>
        /// <param name="chromaCount">Number of chroma features (12 pitch classes by default)</param>
        /// <param name="tuning">Tuning deviation from A440 in fractions of a chroma bin</param>
        /// <param name="centerOctave">If octaveWidth=0, centerOctave is ignored. Otherwise, it's the center of Gaussian window</param>
        /// <param name="octaveWidth">If octaveWidth=0, the shape is rectangular. Otherwise, it's the width of Gaussian window</param>
        /// <param name="norm">Norm: 0 - no normalization, 1 - apply L1-norm, 2 - apply L2-norm</param>
        /// <param name="baseC">If baseC=true, the filter bank will start at 'C'. Otherwise, the filter bank will start at 'A'.</param>
        public static float[][] Chroma(int fftSize,
                                       int samplingRate,
                                       int chromaCount = 12,
                                       double tuning = 0,
                                       double centerOctave = 5.0,
                                       double octaveWidth = 2,
                                       int norm = 2,
                                       bool baseC = true)
        {
            var step = (float)samplingRate / fftSize;

            fftSize = (fftSize / 2) + 1;

            var freqs = Enumerable.Range(1, fftSize - 1)
                                  .Select(i => i * step)
                                  .ToArray();

            var freqBins = new[] { 0.0f }.Concat(
                freqs.Select(
                    f => chromaCount * (float) Scale.HerzToOctave(f, tuning, chromaCount)))
                .ToArray();

            freqBins[0] = freqBins[1] - chromaCount * 1.5f;

            var binWidthBins = Enumerable.Range(1, fftSize - 1)
                                         .Select(i => Math.Max(freqBins[i] - freqBins[i - 1], 1))
                                         .Concat(new[] { 1.0f })
                                         .ToArray();

            var filterbank = new float[chromaCount][];

            for (var i = 0; i < chromaCount; i++)
            {
                filterbank[i] = freqBins.Select(f => f - i).ToArray();
            }

            var chromaCount2 = Math.Round((double)chromaCount / 2, MidpointRounding.AwayFromZero);

            for (var i = 0; i < chromaCount; i++)
            {
                for (var j = 0; j < filterbank[i].Length; j++)
                {
                    var f = (filterbank[i][j] + chromaCount2 + 10 * chromaCount) % chromaCount - chromaCount2;

                    filterbank[i][j] = (float) Math.Exp(-0.5 * Math.Pow(2 * f / binWidthBins[j], 2));
                }
            }

            // normalize if necessary 

            if (norm > 0)
            {
                for (var j = 0; j < fftSize; j++)
                {
                    var fnorm = 0.0f;

                    for (var i = 0; i < chromaCount; i++)
                    {
                        fnorm += (float) Math.Pow(Math.Abs(filterbank[i][j]), norm);
                    }

                    fnorm = (float)(1.0 / Math.Pow(fnorm, 1.0 / norm));

                    for (var i = 0; i < chromaCount; i++)
                    {
                        filterbank[i][j] *= fnorm;
                    }
                }
            }

            // Make Gaussian shape if necessary

            if (octaveWidth > 0)
            {
                for (var i = 0; i < chromaCount; i++)
                {
                    for (var j = 0; j < filterbank[i].Length; j++)
                    {
                        filterbank[i][j] *= (float) Math.Exp(-0.5 * Math.Pow((freqBins[j] / chromaCount - centerOctave) / octaveWidth, 2));
                    }
                }
            }

            if (baseC)
            {
                const int CShiftA = 3;  // C shift relative to A

                for (var k = 0; k < CShiftA; k++)
                {
                    var temp = filterbank[0];

                    for (var i = 0; i < filterbank.Length - 1; i++)
                    {
                        filterbank[i] = filterbank[i + 1];
                    }

                    filterbank[filterbank.Length - 1] = temp;
                }
            }

            return filterbank;
        }

        /// <summary>
        /// Creates overlapping triangular mel filters (as suggested by Malcolm Slaney).
        /// </summary>
        /// <param name="filterCount">Number of mel filters</param>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <param name="normalizeGain">True if gain should be normalized; false if all filters should have same height 1.0</param>
        /// <param name="vtln">VTLN frequency warper</param>
        public static float[][] MelBankSlaney(
            int filterCount, int fftSize, int samplingRate, double lowFreq = 0, double highFreq = 0, bool normalizeGain = true, VtlnWarper vtln = null)
        {
            if (lowFreq < 0)
            {
                lowFreq = 0;
            }
            if (highFreq <= lowFreq)
            {
                highFreq = samplingRate / 2.0;
            }

            var frequencies = UniformBands(Scale.HerzToMelSlaney, Scale.MelToHerzSlaney, filterCount, samplingRate, lowFreq, highFreq, true);

            var filterBank = Triangular(fftSize, samplingRate, frequencies, vtln);

            if (normalizeGain)
            {
                Normalize(filterCount, frequencies, filterBank);
            }

            return filterBank;
        }

        /// <summary>
        /// Creates overlapping trapezoidal bark filters (as suggested by Malcolm Slaney).
        /// </summary>
        /// <param name="filterCount">Number of bark filters</param>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <param name="width">Constant width of each band in Bark</param>
        public static float[][] BarkBankSlaney(
            int filterCount, int fftSize, int samplingRate, double lowFreq = 0, double highFreq = 0, double width = 1)
        {
            if (lowFreq < 0)
            {
                lowFreq = 0;
            }
            if (highFreq <= lowFreq)
            {
                highFreq = samplingRate / 2.0;
            }

            var lowBark = Scale.HerzToBarkSlaney(lowFreq);
            var highBark = Scale.HerzToBarkSlaney(highFreq) - lowBark;

            var herzResolution = (double)samplingRate / fftSize;
            var step = highBark / (filterCount - 1);

            var binBarks = Enumerable.Range(0, fftSize / 2 + 1)
                                     .Select(i => Scale.HerzToBarkSlaney(i * herzResolution))
                                     .ToArray();

            var filterBank = new float[filterCount][];

            var midBark = lowBark;

            for (var i = 0; i < filterCount; i++, midBark += step)
            {
                filterBank[i] = new float[fftSize / 2 + 1];

                for (var j = 0; j < filterBank[i].Length; j++)
                {
                    var lof = binBarks[j] - midBark - 0.5;
                    var hif = binBarks[j] - midBark + 0.5;

                    filterBank[i][j] = (float)Math.Pow(10, Math.Min(0, Math.Min(hif, -2.5 * lof) / width));
                }
            }

            return filterBank;
        }

        /// <summary>
        /// Creates overlapping ERB filters.
        /// </summary>
        /// <param name="erbFilterCount">Number of ERB filters</param>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <param name="normalizeGain">True if gain should be normalized; false if all filters should have same height 1.0</param>
        public static float[][] Erb(
            int erbFilterCount, int fftSize, int samplingRate, double lowFreq = 0, double highFreq = 0, bool normalizeGain = true)
        {
            if (lowFreq < 0)
            {
                lowFreq = 0;
            }
            if (highFreq <= lowFreq)
            {
                highFreq = samplingRate / 2.0;
            }

            // ported from Malcolm Slaney's MATLAB code:

            const double earQ = 9.26449;
            const double minBw = 24.7;
            const double bw = earQ * minBw;
            const int order = 1;

            var t = 1.0 / samplingRate;
            
            var frequencies = new double[erbFilterCount];
            for (var i = 1; i <= erbFilterCount; i++)
            {
                frequencies[erbFilterCount - i] =
                    -bw + Math.Exp(i * (-Math.Log(highFreq + bw) + Math.Log(lowFreq + bw)) / erbFilterCount) * (highFreq + bw);
            }

            var ucirc = new Complex[fftSize / 2 + 1];
            for (var i = 0; i < ucirc.Length; i++)
            {
                ucirc[i] = Complex.Exp((2 * Complex.ImaginaryOne * i * Math.PI) / fftSize);
            }

            var rootPos = Math.Sqrt(3 + Math.Pow(2, 1.5));
            var rootNeg = Math.Sqrt(3 - Math.Pow(2, 1.5));

            var fft = new Fft(fftSize);

            var erbFilterBank = new float[erbFilterCount][];
            
            for (var i = 0; i < erbFilterCount; i++)
            {
                var cf = frequencies[i];
                var erb = Math.Pow(Math.Pow(cf / earQ, order) + Math.Pow(minBw, order), 1.0 / order);
                var b = 1.019 * 2 * Math.PI * erb;

                var theta = 2 * cf * Math.PI * t;
                var itheta = Complex.Exp(2 * Complex.ImaginaryOne * theta);

                var a0 = t;
                var a2 = 0.0;
                var b0 = 1.0;
                var b1 = -2 * Math.Cos(theta) / Math.Exp(b * t);
                var b2 = Math.Exp(-2 * b * t);

                var common = -t * Math.Exp(-b * t);

                var k1 = Math.Cos(theta) + rootPos * Math.Sin(theta);
                var k2 = Math.Cos(theta) - rootPos * Math.Sin(theta);
                var k3 = Math.Cos(theta) + rootNeg * Math.Sin(theta);
                var k4 = Math.Cos(theta) - rootNeg * Math.Sin(theta);

                var a11 = common * k1;
                var a12 = common * k2;
                var a13 = common * k3;
                var a14 = common * k4;

                var gainArg = Complex.Exp(Complex.ImaginaryOne * theta - b * t);

                var gain = (float)Complex.Abs(
                                    (itheta - gainArg * k1) *
                                    (itheta - gainArg * k2) *
                                    (itheta - gainArg * k3) *
                                    (itheta - gainArg * k4) *
                                    Complex.Pow(t * Math.Exp(b * t) / (-1.0/Math.Exp(b*t) + 1 + itheta*(1 - Math.Exp(b*t))), 4.0));

                var filter1 = new IirFilter(new[] { a0, a11, a2 }, new[] { b0, b1, b2 });
                var filter2 = new IirFilter(new[] { a0, a12, a2 }, new[] { b0, b1, b2 });
                var filter3 = new IirFilter(new[] { a0, a13, a2 }, new[] { b0, b1, b2 });
                var filter4 = new IirFilter(new[] { a0, a14, a2 }, new[] { b0, b1, b2 });

                var unitImpulse = DiscreteSignal.Unit(fftSize);

                var chain = new FilterChain(new[] { filter1, filter2, filter3, filter4 });

                var kernel = chain.ApplyTo(unitImpulse);
                kernel.Attenuate(gain);

                erbFilterBank[i] = fft.PowerSpectrum(kernel, false).Samples;
            }

            // normalize gain (by default)

            if (!normalizeGain)
            {
                return erbFilterBank;
            }

            foreach (var filter in erbFilterBank)
            {
                var sum = 0.0;
                for (var j = 0; j < filter.Length; j++)
                {
                    sum += Math.Abs(filter[j] * filter[j]);
                }

                var weight = Math.Sqrt(sum * samplingRate / fftSize);

                for (var j = 0; j < filter.Length; j++)
                {
                    filter[j] = (float)(filter[j] / weight);
                }
            }

            return erbFilterBank;
        }

        /// <summary>
        /// Normalizes weights (so that energies in each band are approx. equal).
        /// </summary>
        /// <param name="filterCount">Number of filters</param>
        /// <param name="frequencies">Array of frequency tuples (left, center, right) for each filter</param>
        /// <param name="filterBank">Filter bank</param>
        public static void Normalize(int filterCount, (double, double, double)[] frequencies, float[][] filterBank)
        {
            for (var i = 0; i < filterCount; i++)
            {
                var (left, _, right) = frequencies[i];

                for (var j = 0; j < filterBank[i].Length; j++)
                {
                    filterBank[i][j] *= 2 / (float)(right - left);
                }
            }
        }

        /// <summary>
        /// Applies filters to spectrum and fills resulting filtered spectrum.
        /// </summary>
        /// <param name="filterbank">Filter bank</param>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="filtered">Spectrum of filtered signal</param>
        public static void Apply(float[][] filterbank, float[] spectrum, float[] filtered)
        {
            for (var i = 0; i < filterbank.Length; i++)
            {
                var en = 0.0f;

                for (var j = 0; j < spectrum.Length; j++)
                {
                    en += filterbank[i][j] * spectrum[j];
                }

                filtered[i] = en;
            }
        }

        /// <summary>
        /// Applies filters to all spectra in given sequence.
        /// </summary>
        /// <param name="filterbank">Filter bank</param>
        /// <param name="spectrogram">Output spectra of filtered signal</param>
        public static float[][] Apply(float[][] filterbank, IList<float[]> spectrogram)
        {
            var filtered = new float[spectrogram.Count][];

            for (var k = 0; k < filtered.Length; k++)
            {
                filtered[k] = new float[filterbank.Length];
            }

            for (var i = 0; i < filterbank.Length; i++)
            {
                for (var k = 0; k < filtered.Length; k++)
                {
                    var en = 0.0f;

                    for (var j = 0; j < spectrogram[i].Length; j++)
                    {
                        en += filterbank[i][j] * spectrogram[k][j];
                    }

                    filtered[k][i] = en;
                }
            }

            return filtered;
        }

        /// <summary>
        /// Applies filters to spectrum and then does Ln() on resulting spectrum.
        /// </summary>
        /// <param name="filterbank">Filter bank</param>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="filtered">Spectrum of filtered signal</param>
        /// <param name="floor">Log-floor (Threshold for log-operation)</param>
        public static void ApplyAndLog(float[][] filterbank, float[] spectrum, float[] filtered, float floor = float.Epsilon)
        {
            for (var i = 0; i < filterbank.Length; i++)
            {
                var en = 0.0f;

                for (var j = 0; j < spectrum.Length; j++)
                {
                    en += filterbank[i][j] * spectrum[j];
                }

                filtered[i] = (float)Math.Log(Math.Max(en, floor));
            }
        }

        /// <summary>
        /// Applies filters to spectrum and then does Log10() on resulting spectrum.
        /// </summary>
        /// <param name="filterbank">Filter bank</param>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="filtered">Spectrum of filtered signal</param>
        /// <param name="floor">Log-floor (Threshold for log-operation)</param>
        public static void ApplyAndLog10(float[][] filterbank, float[] spectrum, float[] filtered, float floor = float.Epsilon)
        {
            for (var i = 0; i < filterbank.Length; i++)
            {
                var en = 0.0f;

                for (var j = 0; j < spectrum.Length; j++)
                {
                    en += filterbank[i][j] * spectrum[j];
                }

                filtered[i] = (float)Math.Log10(Math.Max(en, floor));
            }
        }

        /// <summary>
        /// Applies filters to spectrum and then does 10*Log10() on resulting spectrum 
        /// (added to compare MFCC coefficients with librosa results).
        /// </summary>
        /// <param name="filterbank">Filter bank</param>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="filtered">Spectrum of filtered signal</param>
        /// <param name="minLevel">Threshold for log-operation</param>
        public static void ApplyAndToDecibel(float[][] filterbank, float[] spectrum, float[] filtered, float minLevel = 1e-10f)
        {
            for (var i = 0; i < filterbank.Length; i++)
            {
                var en = 0.0f;

                for (var j = 0; j < spectrum.Length; j++)
                {
                    en += filterbank[i][j] * spectrum[j];
                }

                filtered[i] = (float)Scale.ToDecibelPower(Math.Max(en, minLevel));
            }
        }

        /// <summary>
        /// Applies filters to spectrum and then does Pow(x, power) on resulting spectrum. 
        /// For example, in PLP: power=1/3 (cubic root).
        /// </summary>
        /// <param name="filterbank">Filter bank</param>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="filtered">Spectrum of filtered signal</param>
        /// <param name="power">Power</param>
        public static void ApplyAndPow(float[][] filterbank, float[] spectrum, float[] filtered, double power = 1.0 / 3)
        {
            for (var i = 0; i < filterbank.Length; i++)
            {
                var en = 0.0f;

                for (var j = 0; j < spectrum.Length; j++)
                {
                    en += filterbank[i][j] * spectrum[j];
                }

                filtered[i] = (float)Math.Pow(en, power);
            }
        }
    }
}
