using System;
using System.IO;
using System.Collections.Generic;
using AudioLibrary.NAudio.Wave;
using System.Diagnostics;
using System.Text;

namespace AudioLibrary.NAudio.Wave
{
    /// <summary>This class supports the reading of WAV files,
    /// providing a repositionable WaveStream that returns the raw data
    /// contained in the WAV file
    /// </summary>
    public class WaveFileReader : WaveStream
    {
        private readonly WaveFormat waveFormat;
        private readonly bool ownInput;
        private readonly long dataPosition;
        private readonly long dataChunkLength;
        private readonly object lockObject = new object();
        private Stream waveStream;

        /// <summary>Supports opening a WAV file</summary>
        /// <remarks>The WAV file format is a real mess, but we will only
        /// support the basic WAV file format which actually covers the vast
        /// majority of WAV files out there. For more WAV file format information
        /// visit www.wotsit.org. If you have a WAV file that can't be read by
        /// this class, email it to the NAudio project and we will probably
        /// fix this reader to support it
        /// </remarks>
        public WaveFileReader(String waveFile) :
            this(File.OpenRead(waveFile), true)
        {
        }

        /// <summary>
        /// Creates a Wave File Reader based on an input stream
        /// </summary>
        /// <param name="inputStream">The input stream containing a WAV file including header</param>
        public WaveFileReader(Stream inputStream) :
           this(inputStream, false)
        {
        }

        private WaveFileReader(Stream inputStream, bool ownInput)
        {
            this.waveStream = inputStream;
            var chunkReader = new WaveFileChunkReader();
            try
            {
                chunkReader.ReadWaveHeader(inputStream);
                waveFormat = chunkReader.WaveFormat;
                dataPosition = chunkReader.DataChunkPosition;
                dataChunkLength = chunkReader.DataChunkLength;
                ExtraChunks = chunkReader.RiffChunks;
            }
            catch
            {
                if (ownInput)
                {
                    inputStream.Dispose();
                }

                throw;
            }

            Position = 0;
            this.ownInput = ownInput;
        }

        /// <summary>
        /// Gets a list of the additional chunks found in this file
        /// </summary>
        public List<RiffChunk> ExtraChunks { get; }

        /// <summary>
        /// Gets the data for the specified chunk
        /// </summary>
        public byte[] GetChunkData(RiffChunk chunk)
        {
            long oldPosition = waveStream.Position;
            waveStream.Position = chunk.StreamPosition;
            byte[] data = new byte[chunk.Length];
            waveStream.Read(data, 0, data.Length);
            waveStream.Position = oldPosition;
            return data;
        }

        /// <summary>
        /// Cleans up the resources associated with this WaveFileReader
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources.
                if (waveStream != null)
                {
                    // only dispose our source if we created it
                    if (ownInput)
                    {
                        waveStream.Dispose();
                    }
                    waveStream = null;
                }
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, "WaveFileReader was not disposed");
            }
            // Release unmanaged resources.
            // Set large fields to null.
            // Call Dispose on your base class.
            base.Dispose(disposing);
        }

        /// <summary>
        /// <see cref="WaveStream.WaveFormat"/>
        /// </summary>
        public override WaveFormat WaveFormat => waveFormat;

        /// <summary>
        /// This is the length of audio data contained in this WAV file, in bytes
        /// (i.e. the byte length of the data chunk, not the length of the WAV file itself)
        /// <see cref="WaveStream.WaveFormat"/>
        /// </summary>
        public override long Length => dataChunkLength;

        /// <summary>
        /// Number of Sample Frames  (if possible to calculate)
        /// This currently does not take into account number of channels
        /// Multiply number of channels if you want the total number of samples
        /// </summary>
        public long SampleCount
        {
            get
            {
                if (waveFormat.Encoding == WaveFormatEncoding.Pcm ||
                    waveFormat.Encoding == WaveFormatEncoding.Extensible ||
                    waveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
                {
                    return dataChunkLength / BlockAlign;
                }
                // n.b. if there is a fact chunk, you can use that to get the number of samples
                throw new InvalidOperationException("Sample count is calculated only for the standard encodings");
            }
        }

        /// <summary>
        /// Position in the WAV data chunk.
        /// <see cref="Stream.Position"/>
        /// </summary>
        public override long Position
        {
            get
            {
                return waveStream.Position - dataPosition;
            }
            set
            {
                lock (lockObject)
                {
                    value = Math.Min(value, Length);
                    // make sure we don't get out of sync
                    value -= (value % waveFormat.BlockAlign);
                    waveStream.Position = value + dataPosition;
                }
            }
        }

        /// <summary>
        /// Reads bytes from the Wave File
        /// <see cref="Stream.Read"/>
        /// </summary>
        public override int Read(byte[] array, int offset, int count)
        {
            if (count % waveFormat.BlockAlign != 0)
            {
                throw new ArgumentException(
                    $"Must read complete blocks: requested {count}, block align is {WaveFormat.BlockAlign}");
            }
            lock (lockObject)
            {
                // sometimes there is more junk at the end of the file past the data chunk
                if (Position + count > dataChunkLength)
                {
                    count = (int)(dataChunkLength - Position);
                }
                return waveStream.Read(array, offset, count);
            }
        }

        /// <summary>
        /// Attempts to read the next sample or group of samples as floating point normalised into the range -1.0f to 1.0f
        /// </summary>
        /// <returns>An array of samples, 1 for mono, 2 for stereo etc. Null indicates end of file reached
        /// </returns>
        public float[] ReadNextSampleFrame()
        {
            switch (waveFormat.Encoding)
            {
                case WaveFormatEncoding.Pcm:
                case WaveFormatEncoding.IeeeFloat:
                case WaveFormatEncoding.Extensible: // n.b. not necessarily PCM, should probably write more code to handle this case
                    break;
                default:
                    throw new InvalidOperationException("Only 16, 24 or 32 bit PCM or IEEE float audio data supported");
            }
            var sampleFrame = new float[waveFormat.Channels];
            int bytesToRead = waveFormat.Channels * (waveFormat.BitsPerSample / 8);
            byte[] raw = new byte[bytesToRead];
            int bytesRead = Read(raw, 0, bytesToRead);
            if (bytesRead == 0) return null; // end of file
            if (bytesRead < bytesToRead) throw new InvalidDataException("Unexpected end of file");
            int offset = 0;
            for (int channel = 0; channel < waveFormat.Channels; channel++)
            {
                if (waveFormat.BitsPerSample == 16)
                {
                    sampleFrame[channel] = BitConverter.ToInt16(raw, offset) / 32768f;
                    offset += 2;
                }
                else if (waveFormat.BitsPerSample == 24)
                {
                    sampleFrame[channel] = (((sbyte)raw[offset + 2] << 16) | (raw[offset + 1] << 8) | raw[offset]) / 8388608f;
                    offset += 3;
                }
                else if (waveFormat.BitsPerSample == 32 && waveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
                {
                    sampleFrame[channel] = BitConverter.ToSingle(raw, offset);
                    offset += 4;
                }
                else if (waveFormat.BitsPerSample == 32)
                {
                    sampleFrame[channel] = BitConverter.ToInt32(raw, offset) / (Int32.MaxValue + 1f);
                    offset += 4;
                }
                else
                {
                    throw new InvalidOperationException("Unsupported bit depth");
                }
            }
            return sampleFrame;
        }

        /// <summary>
        /// Attempts to read a sample into a float. n.b. only applicable for uncompressed formats
        /// Will normalise the value read into the range -1.0f to 1.0f if it comes from a PCM encoding
        /// </summary>
        /// <returns>False if the end of the WAV data chunk was reached</returns>
        [Obsolete("Use ReadNextSampleFrame instead (this version does not support stereo properly)")]
        public bool TryReadFloat(out float sampleValue)
        {
            var sf = ReadNextSampleFrame();
            sampleValue = sf != null ? sf[0] : 0;
            return sf != null;
        }
    }


    public class WaveFileChunkReader
    {
        private WaveFormat waveFormat;
        private long dataChunkPosition;
        private long dataChunkLength;
        private List<RiffChunk> riffChunks;
        private readonly bool strictMode;
        private bool isRf64;
        private readonly bool storeAllChunks;
        private long riffSize;

        /// <summary>
        /// Creates a new WaveFileChunkReader
        /// </summary>
        public WaveFileChunkReader()
        {
            storeAllChunks = true;
            strictMode = false;
        }

        /// <summary>
        /// Read the WAV header
        /// </summary>
        public void ReadWaveHeader(Stream stream)
        {
            this.dataChunkPosition = -1;
            this.waveFormat = null;
            this.riffChunks = new List<RiffChunk>();
            this.dataChunkLength = 0;

            var br = new BinaryReader(stream);
            ReadRiffHeader(br);
            this.riffSize = br.ReadUInt32(); // read the file size (minus 8 bytes)

            if (br.ReadInt32() != ChunkIdentifier.ChunkIdentifierToInt32("WAVE"))
            {
                throw new FormatException("Not a WAVE file - no WAVE header");
            }

            if (isRf64)
            {
                ReadDs64Chunk(br);
            }

            int dataChunkId = ChunkIdentifier.ChunkIdentifierToInt32("data");
            int formatChunkId = ChunkIdentifier.ChunkIdentifierToInt32("fmt ");

            // sometimes a file has more data than is specified after the RIFF header
            long stopPosition = Math.Min(riffSize + 8, stream.Length);

            // this -8 is so we can be sure that there are at least 8 bytes for a chunk id and length
            while (stream.Position <= stopPosition - 8)
            {
                Int32 chunkIdentifier = br.ReadInt32();
                var chunkLength = br.ReadUInt32();
                if (chunkIdentifier == dataChunkId)
                {
                    dataChunkPosition = stream.Position;
                    if (!isRf64) // we already know the dataChunkLength if this is an RF64 file
                    {
                        dataChunkLength = chunkLength;
                    }
                    stream.Position += chunkLength;
                }
                else if (chunkIdentifier == formatChunkId)
                {
                    if (chunkLength > Int32.MaxValue)
                        throw new InvalidDataException(string.Format("Format chunk length must be between 0 and {0}.", Int32.MaxValue));
                    waveFormat = WaveFormat.FromFormatChunk(br, (int)chunkLength);
                }
                else
                {
                    // check for invalid chunk length
                    if (chunkLength > stream.Length - stream.Position)
                    {
                        if (strictMode)
                        {
                            Debug.Assert(false, String.Format("Invalid chunk length {0}, pos: {1}. length: {2}",
                                chunkLength, stream.Position, stream.Length));
                        }
                        // an exception will be thrown further down if we haven't got a format and data chunk yet,
                        // otherwise we will tolerate this file despite it having corrupt data at the end
                        break;
                    }
                    if (storeAllChunks)
                    {
                        if (chunkLength > Int32.MaxValue)
                            throw new InvalidDataException(string.Format("RiffChunk chunk length must be between 0 and {0}.", Int32.MaxValue));
                        riffChunks.Add(GetRiffChunk(stream, chunkIdentifier, (int)chunkLength));
                    }
                    stream.Position += chunkLength;
                }

                // All Chunks have to be word aligned.
                // https://www.tactilemedia.com/info/MCI_Control_Info.html
                // "If the chunk size is an odd number of bytes, a pad byte with value zero is
                //  written after ckData. Word aligning improves access speed (for chunks resident in memory)
                //  and maintains compatibility with EA IFF. The ckSize value does not include the pad byte."
                if (((chunkLength % 2) != 0) && (br.PeekChar() == 0))
                {
                    stream.Position++;
                }
            }

            if (waveFormat == null)
            {
                throw new FormatException("Invalid WAV file - No fmt chunk found");
            }
            if (dataChunkPosition == -1)
            {
                throw new FormatException("Invalid WAV file - No data chunk found");
            }
        }

        /// <summary>
        /// http://tech.ebu.ch/docs/tech/tech3306-2009.pdf
        /// </summary>
        private void ReadDs64Chunk(BinaryReader reader)
        {
            int ds64ChunkId = ChunkIdentifier.ChunkIdentifierToInt32("ds64");
            int chunkId = reader.ReadInt32();
            if (chunkId != ds64ChunkId)
            {
                throw new FormatException("Invalid RF64 WAV file - No ds64 chunk found");
            }
            int chunkSize = reader.ReadInt32();
            this.riffSize = reader.ReadInt64();
            this.dataChunkLength = reader.ReadInt64();
            long sampleCount = reader.ReadInt64(); // replaces the value in the fact chunk
            reader.ReadBytes(chunkSize - 24); // get to the end of this chunk (should parse extra stuff later)
        }

        private static RiffChunk GetRiffChunk(Stream stream, Int32 chunkIdentifier, Int32 chunkLength)
        {
            return new RiffChunk(chunkIdentifier, chunkLength, stream.Position);
        }

        private void ReadRiffHeader(BinaryReader br)
        {
            int header = br.ReadInt32();
            if (header == ChunkIdentifier.ChunkIdentifierToInt32("RF64"))
            {
                this.isRf64 = true;
            }
            else if (header != ChunkIdentifier.ChunkIdentifierToInt32("RIFF"))
            {
                throw new FormatException("Not a WAVE file - no RIFF header");
            }
        }

        /// <summary>
        /// WaveFormat
        /// </summary>
        public WaveFormat WaveFormat { get { return this.waveFormat; } }

        /// <summary>
        /// Data Chunk Position
        /// </summary>
        public long DataChunkPosition { get { return this.dataChunkPosition; } }

        /// <summary>
        /// Data Chunk Length
        /// </summary>
        public long DataChunkLength { get { return this.dataChunkLength; } }

        /// <summary>
        /// Riff Chunks
        /// </summary>
        public List<RiffChunk> RiffChunks { get { return this.riffChunks; } }
    }

    /// <summary>
    /// Holds information about a RIFF file chunk
    /// </summary>
    public class RiffChunk
    {
        /// <summary>
        /// Creates a RiffChunk object
        /// </summary>
        public RiffChunk(int identifier, int length, long streamPosition)
        {
            Identifier = identifier;
            Length = length;
            StreamPosition = streamPosition;
        }

        /// <summary>
        /// The chunk identifier
        /// </summary>
        public int Identifier { get; }

        /// <summary>
        /// The chunk identifier converted to a string
        /// </summary>
        public string IdentifierAsString => Encoding.UTF8.GetString(BitConverter.GetBytes(Identifier));

        /// <summary>
        /// The chunk length
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// The stream position this chunk is located at
        /// </summary>
        public long StreamPosition { get; private set; }
    }
    public class ChunkIdentifier
    {
        /// <summary>
        /// Chunk identifier to Int32 (replaces mmioStringToFOURCC)
        /// </summary>
        /// <param name="s">four character chunk identifier</param>
        /// <returns>Chunk identifier as int 32</returns>
        public static int ChunkIdentifierToInt32(string s)
        {
            if (s.Length != 4) throw new ArgumentException("Must be a four character string");
            var bytes = Encoding.UTF8.GetBytes(s);
            if (bytes.Length != 4) throw new ArgumentException("Must encode to exactly four bytes");
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}