using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using UnityEngine;


class NVOggPlayer : ISound
{
    NVorbis.VorbisReader reader;
    public NVOggPlayer(System.IO.Stream s)
    {
        reader = new NVorbis.VorbisReader(s, true);

        isPlaying = true;
    }
    public void Mix(float[] data, int channels,float volume)
    {
        if (reader.SampleRate == 44100 && reader.Channels == channels)
        {
            float[] ndata = new float[data.Length];
            int read = reader.ReadSamples(ndata, 0, ndata.Length);
            for (int i = 0; i < read; i++)
            {
                float b = ndata[i] * volume;
                float a = data[i];
                data[i] = a + b - a * b;
            }
        }
        else if (reader.SampleRate == 44100)
        {
            float[] ndata = new float[data.Length / channels * reader.Channels];
            int read = reader.ReadSamples(ndata, 0, ndata.Length) / reader.Channels;

            for (int i = 0; i < read; i++)
            {
                for (int c = 0; c < channels; c++)
                {
                    float b = ndata[i * reader.Channels + c % reader.Channels] * volume;
                    float a = data[i * channels + c];
                    data[i * channels + c] = a + b - a * b;
                }
            }
        }
        else
        {
            Debug.Log("OnlyPlay 44100,now rate is"+reader.SampleRate);
        }

    }

    public bool isPlaying
    {
        get;
        private set;
    }
}

