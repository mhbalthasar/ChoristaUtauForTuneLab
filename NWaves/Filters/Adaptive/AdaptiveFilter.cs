﻿using NWaves.Filters.Base;
using NWaves.Utils;
using System;

namespace NWaves.Filters.Adaptive
{
    /// <summary>
    /// Abstract class for adaptive filters.
    /// </summary>
    public abstract class AdaptiveFilter : FirFilter
    {
        /// <summary>
        /// Constructs <see cref="AdaptiveFilter"/> of given <paramref name="order"/>.
        /// </summary>
        /// <param name="order">Filter order</param>
        public AdaptiveFilter(int order) : base(new float[order])
        {
            Array.Resize(ref _delayLine, _kernelSize * 2);  // trick for better performance
        }

        /// <summary>
        /// Inits weights of adaptive filter.
        /// </summary>
        /// <param name="weights">Weights (filter kernel)</param>
        public void Init(float[] weights)
        {
            Guard.AgainstInequality(_kernelSize, weights.Length, "Filter order", "Weights array size");
            ChangeKernel(weights);
        }

        /// <summary>
        /// Processes one sample of input and desired signals and adapts filter coefficients.
        /// </summary>
        /// <param name="input">Sample of input signal</param>
        /// <param name="desired">Sample of desired signal</param>
        public abstract float Process(float input, float desired);
    }
}
