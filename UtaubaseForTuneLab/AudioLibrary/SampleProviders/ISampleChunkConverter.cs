using AudioLibrary.NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioLibrary.NAudio.Wave.SampleProviders
{
    interface ISampleChunkConverter
    {
        bool Supports(WaveFormat format);
        void LoadNextChunk(IWaveProvider sourceProvider, int samplePairsRequired);
        bool GetNextSample(out float sampleLeft, out float sampleRight);
    }
}
