using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CSharpSynth.Effects;
using CSharpSynth.Sequencer;
using CSharpSynth.Synthesis;
using CSharpSynth.Midi;


public class UnityMidiSynth : MonoBehaviour
{
    //Public
    //Check the Midi's file folder for different songs
    //public string midiFilePath = "Midis/Groove.mid";
    //Try also: "FM Bank/fm" or "Analog Bank/analog" for some different sounds
    public string bankFilePath = "FM Bank/fm";
    


    public class Sound
    {
        public Sound(string bank,string midifile)
        {
            midiStreamSynthesizer = new StreamSynthesizer(StreamSynthesizer.SampleRateType.High);
            sampleBuffer = new float[midiStreamSynthesizer.BufferSize];
            midiStreamSynthesizer.LoadBank(bank);

            midiSequencer = new MidiSequencer(midiStreamSynthesizer);
            midiSequencer.LoadMidi(midifile, false);
            midiSequencer.Play();
        }
        public bool isPlaying
        {
            get
            {
               
               return midiSequencer.isPlaying;
            }
        }
        public float volume = 1f;
        public bool isLoop
        {
            get
            {
                return midiSequencer.Looping;
            }
            set
            {
                midiSequencer.Looping = isLoop;
            }
        }
        //Private 
        private float[] sampleBuffer = new float[4096];

        private MidiSequencer midiSequencer;
        private StreamSynthesizer midiStreamSynthesizer;
        public void Mix(float[] data, int channels)
        {
            int sample = data.Length / channels;
            //单通道采样即可
            midiStreamSynthesizer.GetNext(sampleBuffer, sample);

            for (int i = 0; i < sample; i++)
            {
                float b = sampleBuffer[i] * volume;
                for (int c = 0; c < channels; c++)
                {
                    float a = data[i * channels + c];
                    data[i * channels + c] = a + b - a * b;
                }
            }
            float[] realout = new float[data.Length];
            float[] imagout = new float[data.Length];

            Ernzo.DSP.FFT.Compute((uint)data.Length, data, null, realout, imagout, false);
        }
    }
    // Awake is called when the script instance
    // is being loaded.
    void Awake()
    {
     


        //These will be fired by the midiSequencer when a song plays. Check the console for messages
        //midiSequencer.NoteOnEvent += new MidiSequencer.NoteOnEventHandler (MidiNoteOnHandler);
        //midiSequencer.NoteOffEvent += new MidiSequencer.NoteOffEventHandler (MidiNoteOffHandler);	

    }
    List<Sound> sounds = new List<Sound>();

    public void Play(string midi)
    {
        Sound s = new Sound(bankFilePath, midi);
        lock (_lock)
        {
            sounds.Add(s);
        }
    }
    public void StopAll()
    {
        lock (_lock)
        {
            sounds.Clear();
        }
    }
    class LockObj
    {

    }
    LockObj _lock = new LockObj();
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
            Sound remove = null;
            lock (_lock)
            {
                foreach (var s in sounds)
                {
                    if (s.isPlaying == false)
                    {
                        //remove = s;
                        //continue;
                    }
                    s.Mix(data, channels);
                }
                sounds.Remove(remove);
            }
        }
        catch
        {
            Debug.LogError("Play Midi err.");
        }

    }


}
