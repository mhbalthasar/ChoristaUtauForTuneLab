using AudioLibrary.NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioLibrary.NAudio.Wave.SampleProviders
{
    public interface ISampleProvider
    {
        /// <summary>
        /// Gets the WaveFormat of this Sample Provider.
        /// </summary>
        /// <value>The wave format.</value>
        WaveFormat WaveFormat { get; }

        /// <summary>
        /// Fill the specified buffer with 32 bit floating point samples
        /// </summary>
        /// <param name="buffer">The buffer to fill with samples.</param>
        /// <param name="offset">Offset into buffer</param>
        /// <param name="count">The number of samples to read</param>
        /// <returns>the number of samples written to the buffer.</returns>
        int Read(float[] buffer, int offset, int count);
    }
}
