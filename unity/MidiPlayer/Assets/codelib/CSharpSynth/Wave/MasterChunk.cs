using System;

namespace CSharpSynth.Wave
{
    public class MasterChunk : IChunk
    {
        public char[] chkID = new char[4];
        public int chksize = 0;
        public char[] WAVEID = new char[4];
        public int WAVEchunks = 0;
        public WaveHelper.WaveChunkType GetChunkType()
        {
            return WaveHelper.WaveChunkType.Master;
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
