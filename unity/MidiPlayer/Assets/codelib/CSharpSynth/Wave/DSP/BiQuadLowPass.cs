//Filter from: http://www.musicdsp.org
//BiQuad lowpass filter by Robert Bristow-Johnson
//link1:http://www.musicdsp.org/files/Audio-EQ-Cookbook.txt
//link2:http://www.musicdsp.org/archive.php?classid=3
using System;

namespace CSharpSynth.Wave.DSP
{
    public class BiQuadLowPass
    {
        //--Variables
        private float[,] temps;
        private float a0, a1, a2;
        private float b0, b1, b2;
        private int channels;
        //--Public Methods
        public BiQuadLowPass(float sampleRate, int channels, float cornerfrequency)
        {
            this.channels = channels;
            temps = new float[channels, 2];
            ResetFilter(sampleRate, cornerfrequency, 1.0f);
        }
        public void ResetFilter(float sampleRate, float cornerFrequency, float Q)
        {
            double w0 = 2.0 * Math.PI * cornerFrequency / sampleRate;
            double cosw0 = Math.Cos(w0);
            double alpha = Math.Sin(w0) / (2.0 * Q);

            this.b0 = (float)((1.0 - cosw0) / 2.0);
            this.b1 = (float)(1.0 - cosw0);
            this.b2 = (float)((1.0 - cosw0) / 2.0);
            this.a0 = (float)(1.0 + alpha);
            this.a1 = (float)(-2.0 * cosw0);
            this.a2 = (float)(1.0 - alpha);
            Array.Clear(temps, 0, channels * temps.GetLength(1));
        }
        public void ApplyFilter(float[,] inputBuffer)
        {
            int bufferlength = inputBuffer.GetLength(1);
            for (int x = 0; x < channels; x++)
            {
                for (int x2 = 0; x2 < bufferlength; x2++)
                {
                    inputBuffer[x, x2] = (b0 / a0) * inputBuffer[x, x2] + (b1 / a0) * temps[x, 0] + (b2 / a0) * temps[x, 1] - (a1 / a0) * temps[x, 0] - (a2 / a0) * temps[x, 1];
                    temps[x, 1] = temps[x, 0];
                    temps[x, 0] = inputBuffer[x, x2];
                }
            }
        }
        //--Static
        /// <summary>
        /// Uses double precision for offline pass on entire set of sampled data
        /// </summary>
        /// <param name="sampleRate"></param>
        /// <param name="cornerFrequency"></param>
        /// <param name="Q"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static float[,] OfflineProcess(double sampleRate, double cornerFrequency, double Q, float[,] data)
        {
            //Set Up Constants
            double w0 = 2 * Math.PI * cornerFrequency / sampleRate;
            double cosw0 = Math.Cos(w0);
            double alpha = Math.Sin(w0) / (2 * Q);
            double b0, b1, b2, a0, a1, a2;
            b0 = (1 - cosw0) / 2;
            b1 = 1 - cosw0;
            b2 = (1 - cosw0) / 2;
            a0 = 1 + alpha;
            a1 = -2 * cosw0;
            a2 = 1 - alpha;
            //-----
            //Apply the filter
            int length = data.GetLength(1);
            float[,] buffer = new float[data.GetLength(0), length];
            for (int x = 0; x < data.GetLength(0); x++)
            {
                for (int n = 0; n < length; n++)
                {
                    if (n == 0)
                    {
                        buffer[x, n] = (float)((b0 / a0) * data[x, n] + (b1 / a0) * 0 + (b2 / a0) * 0 - (a1 / a0) * 0 - (a2 / a0) * 0);
                    }
                    else if (n == 1)
                    {
                        buffer[x, n] = (float)((b0 / a0) * data[x, n] + (b1 / a0) * data[x, n - 1] + (b2 / a0) * 0 - (a1 / a0) * buffer[x, n - 1] - (a2 / a0) * 0);
                    }
                    else
                    {
                        buffer[x, n] = (float)((b0 / a0) * data[x, n] + (b1 / a0) * data[x, n - 1] + (b2 / a0) * data[x, n - 2] - (a1 / a0) * buffer[x, n - 1] - (a2 / a0) * buffer[x, n - 2]);
                    }
                }
            }
            return buffer;
        }
    }
}
