using System;

namespace CSharpSynth.Wave
{
    public class FormatChunk : IChunk
    {
        public char[] chkID = new char[4];
        public Int32 chksize = 0;
        public Int16 wFormatTag = 0;
        public Int16 nChannels = 0;
        public Int32 nSamplesPerSec = 0;
        public Int32 nAvgBytesPerSec = 0;
        public Int16 nBlockAlign = 0;
        public Int16 wBitsPerSample = 0;
        public Int16 cbSize = 0;
        public Int16 wValidBitsPerSample = 0;
        public Int32 dwChannelMask = 0;
        public char[] SubFormat = new char[16];
        public WaveHelper.WaveChunkType GetChunkType()
        {
            return WaveHelper.WaveChunkType.Format;
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
