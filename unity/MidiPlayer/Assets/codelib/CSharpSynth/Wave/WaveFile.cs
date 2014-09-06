namespace CSharpSynth.Wave
{
    public class WaveFile
    {
        //--Variables
        private IChunk[] WaveChunks;
        private byte[] _data;
        //--Public Methods
        public WaveFile(IChunk[] WaveChunks)
        {
            this.WaveChunks = WaveChunks;
            _data = ((DataChunk)GetChunk(WaveHelper.WaveChunkType.Data)).sampled_data;
        }
        public IChunk GetChunk(WaveHelper.WaveChunkType ChunkType)
        {
            for (int x = 0; x < WaveChunks.Length; x++)
            {
                if (WaveChunks[x].GetChunkType() == ChunkType)
                    return WaveChunks[x];
            }
            return null;
        }
        public IChunk GetChunk(int startIndex, WaveHelper.WaveChunkType ChunkType)
        {
            if (startIndex >= WaveChunks.Length)
                return null;
            for (int x = startIndex; x < WaveChunks.Length; x++)
            {
                if (WaveChunks[x].GetChunkType() == ChunkType)
                    return WaveChunks[x];
            }
            return null;
        }
        public byte[] SampleData
        {
            get { return _data; }
        }
        public int NumberOfChunks
        {
            get { return WaveChunks.Length; }
        }
    }
}
