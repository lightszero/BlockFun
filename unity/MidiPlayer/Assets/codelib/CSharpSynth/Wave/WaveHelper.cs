using System;
using CSharpSynth.Wave.DSP;

namespace CSharpSynth.Wave
{
    public class WaveHelper
    {
        //--Enum
        public enum WaveChunkType { Master = 1, Format = 2, Fact = 3, Data = 4 };
        public enum Format_Code { WAVE_FORMAT_PCM = 1, WAVE_FORMAT_IEEE_FLOAT = 3, WAVE_FORMAT_ALAW = 6, WAVE_FORMAT_MULAW = 7, WAVE_FORMAT_EXTENSIBLE = 65534 };
        public enum ChannelType { Mono = -1, Left = 0, Right = 1, Center = 2, Surround = 3, FrontLeft = 4, BackLeft = 5, FrontRight = 6, BackRight = 7, Unknown = 8 };
        //--Public Static Methods
        public static float[,] ReSample(int NewRate, int OldRate, float[,] data)
        {
            if (NewRate == OldRate)
                return data;
            int a = NewRate, b = OldRate, r;
            //finds the biggest factor between the rates
            while (b != 0)
            {
                r = a % b;
                a = b;
                b = r;
            }
            NewRate = NewRate / a;
            OldRate = OldRate / a;
            if (NewRate < OldRate) //DownSample
            {
                if (OldRate % NewRate == 0) //Simple DownSample
                {
                    data = BiQuadLowPass.OfflineProcess(NewRate * a, (NewRate * a) / 2.0, 1, data);
                    data = DownSample(OldRate / NewRate, data);
                }
                else //UpSample then DownSample
                {
                    data = UpSample(NewRate, data);
                    //filter here
                    double upCutOff = (OldRate * a) / 2.0;
                    double downCutOff = (NewRate * a) / 2.0;
                    if (upCutOff <= downCutOff)
                        data = BiQuadLowPass.OfflineProcess(NewRate * a, upCutOff, 1, data);
                    else
                        data = BiQuadLowPass.OfflineProcess(NewRate * a, downCutOff, 1, data);

                    data = DownSample(OldRate, data);
                }
            }
            else if (NewRate > OldRate) //UpSample
            {
                if (NewRate % OldRate == 0) //Simple UpSample
                {
                    data = UpSample(NewRate / OldRate, data);
                    data = BiQuadLowPass.OfflineProcess(NewRate * a, (OldRate * a) / 2.0, 1, data);
                }
                else //UpSample then DownSample
                {
                    data = UpSample(NewRate, data);
                    //filter here
                    double upCutOff = (OldRate * a) / 2.0;
                    double downCutOff = (NewRate * a) / 2.0;
                    if (upCutOff <= downCutOff)
                        data = BiQuadLowPass.OfflineProcess(NewRate * a, upCutOff, 1, data);
                    else
                        data = BiQuadLowPass.OfflineProcess(NewRate * a, downCutOff, 1, data);

                    data = DownSample(OldRate, data);
                }
            }
            return data;
        }
        public static float[,] DownSample(int factor, float[,] data)
        {//skipping samples
            if (factor == 1)
                return data;
            int oldLen = data.GetLength(1);
            int newLen = (int)(oldLen * (1.00f / factor));
            float[,] newData = new float[data.GetLength(0), newLen];

            for (int x = 0; x < data.GetLength(0); x++)
            {
                for (int i = 0; i < newLen; ++i)
                {
                    newData[x, i] = data[x, i * factor];
                }
            }
            return newData;
        }
        public static float[,] UpSample(int factor, float[,] data)
        {//zero padding
            if (factor == 1)
                return data;
            int oldLen = data.GetLength(1);
            int newLen = oldLen * factor;
            float[,] newData = new float[data.GetLength(0), newLen];

            for (int x = 0; x < data.GetLength(0); x++)
            {
                int cc = 0;
                while (cc < newLen)
                {
                    newData[x, cc] = data[x, cc / factor];
                    for (int i = 0; i < factor - 1; ++i)
                    {
                        cc++;
                        newData[x, cc] = 0;
                    }
                    cc++;
                }
            }
            return newData;
        }
        public static void ChangeBitsPerSample(FormatChunk fmtchk, DataChunk datachk, int bitsPerSample)
        {
            if (fmtchk.wBitsPerSample == bitsPerSample)
                return;
            float change = bitsPerSample / fmtchk.wBitsPerSample;
            
            float[,] samples = GetSampleData(fmtchk, datachk);
            datachk.sampled_data = GetRawData(samples, bitsPerSample);
            datachk.chksize = datachk.sampled_data.Length;
            fmtchk.wBitsPerSample = (short)bitsPerSample;
            fmtchk.nBlockAlign = (short)(fmtchk.nChannels * (bitsPerSample / 8));
            fmtchk.nAvgBytesPerSec = fmtchk.nBlockAlign * fmtchk.nSamplesPerSec;
        }
        public static float[,] GetSampleData(FormatChunk fmtchk, DataChunk datachk)
        {
            int bytesPerSample = fmtchk.wBitsPerSample / 8;
            int channels = fmtchk.nChannels;
            int samplePerChannel = (datachk.sampled_data.Length / bytesPerSample) / channels;
            float[,] samples = new float[channels, samplePerChannel];
            for (int x = 0; x < samplePerChannel; x++)
            {
                for (int i = 0; i < channels; i++)
                {
                    switch (bytesPerSample)
                    {
                        case 1:
                            samples[i, x] = (datachk.sampled_data[(x * channels) + i] - 128) / 128.0f;
                            break;
                        case 2:
                            samples[i, x] = BitConverter.ToInt16(datachk.sampled_data,(x * 2 * channels) + (i * 2)) / (float)Int16.MaxValue;
                            break;
                        case 3:
                            samples[i, x] = ToInt24(datachk.sampled_data, (x * 3 * channels) + (i * 3)) / (float)8388607;
                            break;
                        case 4:
                            samples[i, x] = BitConverter.ToInt32(datachk.sampled_data, (x * 4 * channels) + (i * 4)) / (float)Int32.MaxValue;
                            break;
                    }
                }
            }
            return samples;
        }
        public static byte[] GetRawData(float[,] sampledata, int bitsPerSample)
        {
            int samplesPerChannel = sampledata.GetLength(1);
            int channels = sampledata.GetLength(0);
            int size = sampledata.GetLength(0) * samplesPerChannel * (bitsPerSample / 8);
            int counter = 0;
            int increment = bitsPerSample / 8;
            byte[] rawdata = new byte[size];
            for (int x = 0; x < samplesPerChannel; x++)
            {
                for (int c = 0; c < channels; c++)
                {
                    byte[] buffer;
                    switch (increment)
                    {
                        case 1:
                            rawdata[counter] = (byte)((sampledata[c, x] * 128.0f) + 128.0f);
                            break;
                        case 2:
                            buffer = BitConverter.GetBytes((Int16)(sampledata[c, x] * Int16.MaxValue));
                            rawdata[counter] = buffer[0];
                            rawdata[counter + 1] = buffer[1];
                            break;
                        case 3:
                            buffer = Int24toBytes((Int32)(sampledata[c, x] * 8388607));
                            rawdata[counter] = buffer[0];
                            rawdata[counter + 1] = buffer[1];
                            rawdata[counter + 2] = buffer[2];
                            break;
                        case 4:
                            buffer = BitConverter.GetBytes((Int32)(sampledata[c, x] * Int32.MaxValue));
                            rawdata[counter] = buffer[0];
                            rawdata[counter + 1] = buffer[1];
                            rawdata[counter + 2] = buffer[2];
                            rawdata[counter + 3] = buffer[3];
                            break;
                    }
                    counter += increment;
                }
            }
            return rawdata;
        }
        public static Int32 ToInt24(byte[] buffer, int index)
        {
            if (buffer[index+2] > 127)
              return BitConverter.ToInt32(new byte[] { buffer[index], buffer[index+1], buffer[index+2], 255 }, 0);
            else
              return BitConverter.ToInt32(new byte[] { buffer[index], buffer[index+1], buffer[index+2], 0 }, 0);
        }
        public static byte[] Int24toBytes(Int32 value)
        {
            byte[] bb = BitConverter.GetBytes(value);
            byte[] bytes = new byte[3];
            bytes[0] = bb[0];
            bytes[1] = bb[1];
            bytes[2] = bb[2];
            return bytes;
        }
    }
}
