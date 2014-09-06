using System;

namespace CSharpSynth.Wave
{
    public interface IChunk
    {
        WaveHelper.WaveChunkType GetChunkType();
        String GetChunkId();
        int GetChunkSize();
    }
}
