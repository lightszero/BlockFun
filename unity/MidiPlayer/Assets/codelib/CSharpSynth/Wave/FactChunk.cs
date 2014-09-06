using System;

namespace CSharpSynth.Wave
{
    public class FactChunk : IChunk
    {
        public char[] chkID = new char[4];
        public int chksize = 0;
        public int dwSampleLength = 0;
        public WaveHelper.WaveChunkType GetChunkType()
        {
            return WaveHelper.WaveChunkType.Fact;
        }
        public String GetChunkId()
        {
            return new String(chkID);
        }
        public int GetChunkSize()
        {
            return chksize;
        }
    }
}
