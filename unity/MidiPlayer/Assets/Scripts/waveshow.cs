using UnityEngine;
using System.Collections;


public class waveshow : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    /// <summary>
    /// 插入这个函数，Unity就会在一个新线程上调用
    /// 我们可以通过DSP混音来播放Midi
    /// 这个玩意出错后果很严重
    /// </summary>
    /// <param name="data"></param>
    /// <param name="channels"></param>

    public class lobj
    {

    }
    lobj lockOBJ = new lobj();

    public float[] GetDataSafe()
    {
        float[] nd = new float[NUM_FREQUENCY];
        lock (lockOBJ)
        {
            data.CopyTo(nd, 0);
        }
        return nd;
    }
    void SetDataSafe(float[] nd)
    {
        lock (lockOBJ)
        {
            nd.CopyTo(data, 0);
        }
    }

    private const int NUM_FREQUENCY = 19;
    private int[] METER_FREQUENCY = new int[NUM_FREQUENCY] { 30, 60, 80, 90, 100, 150, 200, 330, 480, 660, 880, 1000, 1500, 2000, 3000, 5000, 8000, 12000, 16000 };
    float[] data = new float[NUM_FREQUENCY];
    //float dmax = 0;
    private void OnAudioFilterRead(float[] data, int channels)
    {
        try
        {
            //Debug.Log(dmax);
            float[] sdata = new float[data.Length / channels];
            for (int i = 0; i < sdata.Length; i++)
            {
                sdata[i] = data[i * channels];
            }
            float[] realout = new float[sdata.Length];
            float[] imagout = new float[sdata.Length];
            float[] pamlout = new float[sdata.Length];
            Ernzo.DSP.FFT.Compute((uint)sdata.Length, sdata, null, realout, imagout, false);
            Ernzo.DSP.FFT.Norm((uint)sdata.Length, realout, imagout, pamlout);
            //for (int i = 0; i < pamlout.Length; i++)
            //{

            //    dmax = Mathf.Max(dmax, pamlout[i]);
            //}
            float[] odata = new float[NUM_FREQUENCY];
            int centerFreq = 22050;
            for (int i = 0; i < NUM_FREQUENCY; ++i)
            {
                if (METER_FREQUENCY[i] > centerFreq)
                {
                    odata[i] = 0;
                }
                else
                {
                    //int indice = (int)((float)i / (float)NUM_FREQUENCY * pamlout.Length);
                    int indice = (int)((float)METER_FREQUENCY[i] * (float)pamlout.Length / (float)44100);
                    if (indice >= pamlout.Length) indice = pamlout.Length - 1;
                    float v = Mathf.Sqrt(pamlout[indice]);
                    v = Mathf.Sqrt(v);
                    //float v = Mathf.Log10(pamlout[indice]);

                    odata[i] = v;
                }

                //Debug.Log(indice);
                // odata[i] = pamlout[indice];

            }
            SetDataSafe(odata);
        }
        catch
        {
            Debug.LogError("DSP err.");
        }

    }
}
