using System;

namespace CSharpSynth.Wave
{
    public class DataChunk : IChunk
    {
        //--Variables
        public char[] chkID = new char[4];
        public int chksize = 0;
        public byte[] sampled_data;
        public byte pad = 0;
        //--Public Methods
        public System.IO.Stream GetDataStream()
        {
            return new System.IO.MemoryStream(sampled_data);
        }
        public WaveHelper.WaveChunkType GetChunkType()
        {
            return WaveHelper.WaveChunkType.Data;
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
