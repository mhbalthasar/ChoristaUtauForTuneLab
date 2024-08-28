﻿using NWaves.Filters.Base;

namespace NWaves.Filters
{
    /// <summary>
    /// Represents DC removal IIR filter.
    /// </summary>
    public class DcRemovalFilter : IirFilter
    {
        /// <summary>
        /// R parameter.
        /// </summary>
        private readonly float _r;

        // Delay line (consists of two samples)
        
        private float _in1;
        private float _out1;

        /// <summary>
        /// Constructs <see cref="DcRemovalFilter"/>.
        /// </summary>
        /// <param name="r">R coefficient (usually in [0.9, 1] range)</param>
        public DcRemovalFilter(double r = 0.995) : base(new [] { 1.0, -1 }, new [] { 1.0, -r })
        {
            _r = (float)r;
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            var output = sample - _in1 + _r * _out1;

            _in1 = sample;
            _out1 = output;

            return output;
        }

        /// <summary>
        /// Resets filter.
        /// </summary>
        public override void Reset()
        {
            _in1 = _out1 = 0;
        }
    }
}
