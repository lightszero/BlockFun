using UnityEngine;
using System.Collections;
using OggSharp;
using System.Collections.Generic;

public class OggPlayer : ISound
{
    OggSharp.OggDecoder decoder = new OggSharp.OggDecoder();
    IEnumerator it = null;
    public OggPlayer(System.IO.Stream s, bool seek = false)
    {
        decoder.Initialize(s, seek);
        it = decoder.GetEnumerator();


        Debug.Log(now);
        isPlaying = true;
        Debug.Log("OggPlayer");
    }
    PCMChunk? now = null;
    int chunkseed = 0;
    public void Mix(float[] data, int channels)
    {

        int len = data.Length / channels;

        for (int i = 0; i < len; i++)
        {
            if (isPlaying)
            {
                if (now == null)
                {
                    if (it.MoveNext())
                    {

                        now = (PCMChunk)it.Current;
                        Debug.Log(now.Value.Length + "," + now.Value.Bytes.Length);
                    }
                    else
                    {
                        isPlaying = false;
                        break;
                    }
                }

                for (int c = 0; c < channels; c++)
                {
                    float a = 0;
                    if ((i + chunkseed) * channels < now.Value.Length/2)
                    {

                        int ii=((i + chunkseed) * channels + c)*2;
                        short bv = (short)( now.Value.Bytes[ii] + now.Value.Bytes[ii + 1] * 256);

                        a = (float)bv / (float)short.MaxValue;

                        
                    }
                    else
                    {

                        if (it.MoveNext())
                        {
                            //Debug.Log("next");
                            now = (PCMChunk)it.Current;
                            chunkseed = 0;
                        }
                        else
                        {
                            isPlaying = false;
                            break;
                        }

                    }



                    float b = data[i * channels + c];
                    data[i * channels + c] = a + b - a * b;
                }
            }
        }
        chunkseed += len;
        //Debug.Log("mix");

    }

    public bool isPlaying
    {
        get;
        private set;
    }
}
