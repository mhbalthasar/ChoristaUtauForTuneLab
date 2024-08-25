// created on 27/12/2002 at 20:20
using AudioLibrary.NAudio.Wave;
using System;
using System.IO;

// ReSharper disable once CheckNamespace
namespace AudioLibrary.NAudio.Wave
{
    public interface IWaveProvider
    {
        /// <summary>
        /// Gets the WaveFormat of this WaveProvider.
        /// </summary>
        /// <value>The wave format.</value>
        WaveFormat WaveFormat { get; }

        /// <summary>
        /// Fill the specified buffer with wave data.
        /// </summary>
        /// <param name="buffer">The buffer to fill of wave data.</param>
        /// <param name="offset">Offset into buffer</param>
        /// <param name="count">The number of bytes to read</param>
        /// <returns>the number of bytes written to the buffer.</returns>
        int Read(byte[] buffer, int offset, int count);
    }
    /// <summary>
    /// Base class for all WaveStream classes. Derives from stream.
    /// </summary>
    public abstract class WaveStream : Stream, IWaveProvider
    {
        /// <summary>
        /// Retrieves the WaveFormat for this stream
        /// </summary>
        public abstract WaveFormat WaveFormat { get; }

        // base class includes long Position get; set
        // base class includes long Length get
        // base class includes Read
        // base class includes Dispose

        /// <summary>
        /// We can read from this stream
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        /// We can seek within this stream
        /// </summary>
        public override bool CanSeek => true;

        /// <summary>
        /// We can't write to this stream
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// Flush does not need to do anything
        /// See <see cref="Stream.Flush"/>
        /// </summary>
        public override void Flush() { }

        /// <summary>
        /// An alternative way of repositioning.
        /// See <see cref="Stream.Seek"/>
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
                Position = offset;
            else if (origin == SeekOrigin.Current)
                Position += offset;
            else
                Position = Length + offset;
            return Position;
        }

        /// <summary>
        /// Sets the length of the WaveStream. Not Supported.
        /// </summary>
        /// <param name="length"></param>
        public override void SetLength(long length)
        {
            throw new NotSupportedException("Can't set length of a WaveFormatString");
        }

        /// <summary>
        /// Writes to the WaveStream. Not Supported.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Can't write to a WaveFormatString");
        }

        /// <summary>
        /// The block alignment for this wavestream. Do not modify the Position
        /// to anything that is not a whole multiple of this value
        /// </summary>
        public virtual int BlockAlign => WaveFormat.BlockAlign;

        /// <summary>
        /// Moves forward or backwards the specified number of seconds in the stream
        /// </summary>
        /// <param name="seconds">Number of seconds to move, can be negative</param>
        public void Skip(int seconds)
        {
            long newPosition = Position + WaveFormat.AverageBytesPerSecond * seconds;
            if (newPosition > Length)
                Position = Length;
            else if (newPosition < 0)
                Position = 0;
            else
                Position = newPosition;
        }

        /// <summary>
        /// The current position in the stream in Time format
        /// </summary>
        public virtual TimeSpan CurrentTime
        {
            get
            {
                return TimeSpan.FromSeconds((double)Position / WaveFormat.AverageBytesPerSecond);
            }
            set
            {
                Position = (long)(value.TotalSeconds * WaveFormat.AverageBytesPerSecond);
            }
        }

        /// <summary>
        /// Total length in real-time of the stream (may be an estimate for compressed files)
        /// </summary>
        public virtual TimeSpan TotalTime
        {
            get
            {
                return TimeSpan.FromSeconds((double)Length / WaveFormat.AverageBytesPerSecond);
            }
        }

        /// <summary>
        /// Whether the WaveStream has non-zero sample data at the current position for the 
        /// specified count
        /// </summary>
        /// <param name="count">Number of bytes to read</param>
        public virtual bool HasData(int count)
        {
            return Position < Length;
        }
    }

    /// <summary>
    /// An interface for WaveStreams which can report notification of individual samples
    /// </summary>
    public interface ISampleNotifier
    {
        /// <summary>
        /// A sample has been detected
        /// </summary>
        event EventHandler<SampleEventArgs> Sample;
    }

    /// <summary>
    /// Sample event arguments
    /// </summary>
    public class SampleEventArgs : EventArgs
    {
        /// <summary>
        /// Left sample
        /// </summary>
        public float Left { get; set; }
        /// <summary>
        /// Right sample
        /// </summary>
        public float Right { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SampleEventArgs(float left, float right)
        {
            Left = left;
            Right = right;
        }
    }
}