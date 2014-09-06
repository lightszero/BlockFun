namespace CSharpSynth.Effects
{
    public abstract class BasicAudioEffect
    {
        //--Variables
        private float[,] effectBuffer;
        //--Public Methods
        public BasicAudioEffect()
        {
        }
        public abstract void doEffect(float[,] inputBuffer,int length);
        //--Public Properties
        public float[,] EffectBuffer
        {
            get { return effectBuffer; }
            set { effectBuffer = value; }
        }
        //--Static Methods
        public static void Average(float[,] buffer)
        {
            for (int x = 0; x < buffer.GetLength(0); x++)
            {
                int End = buffer.GetLength(1) - 1;
                float startVal = (buffer[x, 0] + buffer[x, 1]) / 2.0f;
                float endVal = (buffer[x, End] + buffer[x, End - 1]) / 2.0f;
                for (int y = 1; y < End; y++)
                {
                    buffer[x, y] = (buffer[x, y - 1] + buffer[x, y] + buffer[x, y + 1]) / 3.0f;
                }
                buffer[x, 0] = startVal;
                buffer[x, End] = endVal;
            }
        }
        public static void Gate(float[,] buffer, float lowerLimit, float higherLimit)
        {
            for (int x = 0; x < buffer.GetLength(0); x++)
            {
                int End = buffer.GetLength(1);
                for (int y = 0; y < End; y++)
                {
                    if (buffer[x, y] < lowerLimit || buffer[x, y] > higherLimit)
                        buffer[x, y] = 0.0f;
                }
            }
        }
    }
}
