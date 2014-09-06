using System;

namespace CSharpSynth.Effects
{
    public class dBMeter : BasicAudioEffect
    {
        //--Variables
        private float L_PeakdB = 1.0f;
        private float L_CurrentdB = 0.0f;
        private float R_PeakdB = 1.0f;
        private float R_CurrentdB = 0.0f;
        private bool useFastTest = false;
        //--Public Properties
        public float LeftPeak
        {
            get { return L_PeakdB; }
            set { L_PeakdB = value; }
        }
        public float RightPeak
        {
            get { return R_PeakdB; }
            set { R_PeakdB = value; }
        }
        public float Left_dBLevel
        {
            get { return L_CurrentdB; }
        }
        public float Right_dBLevel
        {
            get { return R_CurrentdB; }
        }
        public bool UseFastVersion
        {
            get { return useFastTest; }
            set
            {
                if (value)
                {
                    L_PeakdB = 50f;
                    R_PeakdB = 50f;
                }
                else
                {
                    L_PeakdB = 1f;
                    R_PeakdB = 1f;
                }
                useFastTest = value;
            }
        }
        //--Public Methods
        public override void doEffect(float[,] inputBuffer,int length)
        {
            if (useFastTest)
                fastTest(inputBuffer,length);
            else
                slowTest(inputBuffer, length);
        }
        //--Private Methods
        private void fastTest(float[,] inputBuffer,int length)
        {
            int channels = inputBuffer.GetLength(0);
            for (int x = 0; x < channels; x++)
            {
                float dB = inputBuffer[x, 0] * inputBuffer[x, 0];
                if (x == 0)
                {
                    L_CurrentdB = Math.Abs((float)(20 * Math.Log10(Math.Sqrt(dB))));
                    //if (L_CurrentdB > L_PeakdB && !float.IsInfinity(L_CurrentdB))
                    //    L_PeakdB = L_CurrentdB;
                }
                else
                {
                    R_CurrentdB = Math.Abs((float)(20 * Math.Log10(Math.Sqrt(dB))));
                    //if (R_CurrentdB > R_PeakdB && !float.IsInfinity(R_CurrentdB))
                    //    R_PeakdB = R_CurrentdB;
                }
            }
        }
        private void slowTest(float[,] inputBuffer,int length)
        {
            int channels = inputBuffer.GetLength(0);
            //int samples = inputBuffer.GetLength(1);
            int samples = length;
            for (int x = 0; x < channels; x++)
            {
                float dB = 0.0f;
                for (int y = 0; y < samples; y++)
                {
                    dB += (inputBuffer[x, y] * inputBuffer[x, y]);
                }
                if (x == 0)
                {
                    L_CurrentdB = Math.Abs((float)(20 * Math.Log10(Math.Sqrt(dB / samples * 2))));
                    if (L_CurrentdB > L_PeakdB && !float.IsInfinity(L_CurrentdB))
                        L_PeakdB = L_CurrentdB;
                }
                else
                {
                    R_CurrentdB = Math.Abs((float)(20 * Math.Log10(Math.Sqrt(dB / samples * 2))));
                    if (R_CurrentdB > R_PeakdB && !float.IsInfinity(R_CurrentdB))
                        R_PeakdB = R_CurrentdB;
                }
            }
            if (channels == 1)
            {
                R_CurrentdB = L_CurrentdB;
                R_PeakdB = L_PeakdB;
            }
        }
    }
}
