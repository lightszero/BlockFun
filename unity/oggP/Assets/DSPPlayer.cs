using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class DSPPlayer : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    class lockobj
    {

    }

    public void Play(ISound sound)
    {
        lock (_lock)
        {
            sounds.Add(sound);
        }

    }
    List<ISound> sounds = new List<ISound>();
    lockobj _lock = new lockobj();
    /// <summary>
    /// 插入这个函数，Unity就会在一个新线程上调用
    /// 我们可以通过DSP混音来播放Midi
    /// 这个玩意出错后果很严重
    /// </summary>
    /// <param name="data"></param>
    /// <param name="channels"></param>
    private void OnAudioFilterRead(float[] data, int channels)
    {
        try
        {
            ISound remove = null;
            lock (_lock)
            {
                foreach (var s in sounds)
                {
                    if (s.isPlaying == false)
                    {
                        remove = s;
                        continue;
                    }
                    s.Mix(data, channels);
                }
                sounds.Remove(remove);
            }
        }
        catch
        {
            Debug.LogError("DSP err.");
        }

    }

}
public interface ISound
{
    //混音
    void Mix(float[] data, int channels);

    bool isPlaying
    {
        get;
    }
}