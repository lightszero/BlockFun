using System;
using System.Collections.Generic;
using CSharpSynth.Banks;
using CSharpSynth.Sequencer;
using CSharpSynth.Effects;
using UnityEngine;

namespace CSharpSynth.Synthesis
{
    public class StreamSynthesizer
    {
        //--Variables
        private InstrumentBank bank;
        private float[,] sampleBuffer;
        private int rawBufferLength;
        private Voice[] voicePool;
        private LinkedList<Voice> activeVoices;
        private Stack<Voice> freeVoices;
        private Dictionary<NoteRegistryKey, List<Voice>> keyRegistry;
        private float[] panPositions_;
        private float[] volPositions_;
        private double[] tunePositions_;
        private MidiSequencer seq;
        private List<BasicAudioEffect> effects;
        //Set "once parameters"
        private int audioChannels = 1;
        private int sampleRate = 44100;
        private int samplesperBuffer = 2000;
        private int polyphony = 40; //total number of voices available
        private int maxnotepoly = 2; //how many of the same note can be playing at once
        //Tweakable Parameters, anytime via properties
        private float MainVolume = 1.0f; //Not too high or will cause clipping
        //--Public Properties
        public int BufferSize
        {
            get { return rawBufferLength; }
        }
        public float[] PanPositions
        {
            get { return panPositions_; }
        }
        public float[] VolPositions
        {
            get { return volPositions_; }
        }
        public double[] TunePositions
        {
            get { return tunePositions_; }
        }
        public int MaxPolyPerNote
        {
            get { return maxnotepoly; }
            set { maxnotepoly = value; }
        }
        public float MasterVolume
        {
            get { return MainVolume; }
            set { MainVolume = SynthHelper.Clamp(value, 0.0f, 1.0f); }
        }
        public int SampleRate
        {
            get { return sampleRate; }
        }
        public int Channels
        {
            get { return audioChannels; }
        }
        public InstrumentBank SoundBank
        {
            get { return bank; }
        }
        //--Public Methods
        //public StreamSynthesizer(int sampleRate, int audioChannels, int bufferSizeInMilliseconds, int maxpoly)
        //{
        //    this.sampleRate = sampleRate;
        //    this.audioChannels = audioChannels;
        //    this.samplesperBuffer = (int)((sampleRate / 1000.0) * bufferSizeInMilliseconds);
        //    this.polyphony = maxpoly;
        //    setupSynth();
        //}
        //UnitySynth
        public enum SampleRateType
        {
            High = 44100,
            Half = 22050,
        }
        public StreamSynthesizer(SampleRateType sampleRate, int bufferSize = 4096, int maxpoly = 60)
        {
            this.sampleRate = (int)sampleRate;
            this.audioChannels = 1;
            //UnitySynth
            this.samplesperBuffer = bufferSize;
            this.polyphony = maxpoly;
            setupSynth();
        }

        public bool LoadBank(string filename)
        {
            //UnitySynth
            //try
            //{
            //    BankManager.addBank(new InstrumentBank(sampleRate, filename));
            //    SwitchBank(BankManager.Count - 1);
            //}
            //catch (Exception ex)
            //{
            //    Debug.Log("Bank load error!\n" + ex.Message + "\n\n" + ex.StackTrace);
            //    return false;
            //}
            //UnitySynth
            BankManager.addBank(new InstrumentBank(sampleRate, filename));
            SwitchBank(BankManager.Count - 1);
            return true;
        }
        public bool UnloadBank(int index)
        {
            if (index < BankManager.Count)
            {
                if (BankManager.Banks[index] == bank)
                    bank = null;
                BankManager.removeBank(index);
                return true;
            }
            return false;
        }
        public bool UnloadBank()
        {
            if (bank != null)
            {
                BankManager.removeBank(bank);
                return true;
            }
            return false;
        }
        public void SwitchBank(int index)
        {
            if (index < BankManager.Count)
                this.bank = BankManager.getBank(index);
        }
        public void setPan(int channel, float position)
        {
            if (channel > -1 && channel < panPositions_.Length && position >= -1.00f && position <= 1.00f)
                panPositions_[channel] = position;
        }
        public void setVolume(int channel, float position)
        {
            if (channel > -1 && channel < volPositions_.Length && position >= 0.00f && position <= 1.00f)
                volPositions_[channel] = position;
        }
        public void setPitchBend(int channel, float semitones)
        {
            if (channel > -1 && channel < tunePositions_.Length && semitones >= -12.00f && semitones <= 12.00f)
            {
                tunePositions_[channel] = semitones;
            }
        }
        public void setSequencer(MidiSequencer sequencer)
        {
            this.seq = sequencer;
        }
        public void resetSynthControls()
        {
            //Reset Pan Positions back to 0.0f
            Array.Clear(panPositions_, 0, panPositions_.Length);
            //Set Tuning Positions back to 0.0f
            Array.Clear(tunePositions_, 0, tunePositions_.Length);
            //Reset Vol Positions back to 1.00f
            for (int x = 0; x < volPositions_.Length; x++)
                volPositions_[x] = 1.00f;
        }
        public void Dispose()
        {
            Stop();
            sampleBuffer = null;
            voicePool = null;
            activeVoices.Clear();
            freeVoices.Clear();
            keyRegistry.Clear();
            effects.Clear();
        }
        public void Stop()
        {
            NoteOffAll(true);
        }
        public void NoteOn(int channel, int note, int velocity, int program)
        {
            // Grab a free voice
            Voice freeVoice = getFreeVoice();
            if (freeVoice == null)
            {
                // If there are no free voices steal an active one.
                freeVoice = getUsedVoice(activeVoices.First.Value.getKey());
                // If there are no voices to steal then leave this method.
                if (freeVoice == null)
                    return;
            }
            // Create a key for this event
            NoteRegistryKey r = new NoteRegistryKey((byte)channel, (byte)note);
            // Get the correct instrument depending if it is a drum or not
            if (channel == 9)
                freeVoice.setInstrument(bank.getInstrument(program, true));
            else
                freeVoice.setInstrument(bank.getInstrument(program, false));
            // Check if key exists
            if (keyRegistry.ContainsKey(r))
            {
                if (keyRegistry[r].Count >= maxnotepoly)
                {
                    keyRegistry[r][0].Stop();
                    keyRegistry[r].RemoveAt(0);
                }
                keyRegistry[r].Add(freeVoice);
            }
            else//The first noteOn of it's own type will create a list for multiple occurences
            {
                List<Voice> Vlist = new List<Voice>(maxnotepoly);
                Vlist.Add(freeVoice);
                keyRegistry.Add(r, Vlist);
            }
            freeVoice.Start(channel, note, velocity);
            activeVoices.AddLast(freeVoice);
        }
        public void NoteOff(int channel, int note)
        {
            NoteRegistryKey r = new NoteRegistryKey((byte)channel, (byte)note);
            List<Voice> voice;
            if (keyRegistry.TryGetValue(r, out voice))
            {
                if (voice.Count > 0)
                {
                    voice[0].Stop();
                    voice.RemoveAt(0);
                }
            }
        }
        public void NoteOffAll(bool immediate)
        {
            if (keyRegistry.Keys.Count == 0 && activeVoices.Count == 0)
                return;
            LinkedListNode<Voice> node = activeVoices.First;
            while (node != null)
            {
                if (immediate)
                    node.Value.StopImmediately();
                else
                    node.Value.Stop();
                node = node.Next;
            }
            keyRegistry.Clear();
        }
        //public void GetNext(byte[] buffer,int length)
        //{//Call this to process the next part of audio and return it in raw form.
        //    ClearWorkingBuffer(length);
        //    FillWorkingBuffer(length);
        //    for (int x = 0; x < effects.Count; x++)
        //    {
        //        effects[x].doEffect(sampleBuffer, length);
        //    }
        //    ConvertBuffer(sampleBuffer, buffer,length);
        //}

        //UnitySynth
        public void GetNext(float[] buffer,int length)
        {//Call this to process the next part of audio and return it in raw form.
            ClearWorkingBuffer(length);
            FillWorkingBuffer(length);
            for (int x = 0; x < effects.Count; x++)
            {
                effects[x].doEffect(sampleBuffer, length);
            }
            ConvertBuffer(sampleBuffer, buffer, length);
        }

        public void AddEffect(BasicAudioEffect effect)
        {
            effects.Add(effect);
        }
        public void RemoveEffect(int index)
        {
            effects.RemoveAt(index);
        }
        public void ClearEffects()
        {
            effects.Clear();
        }
        //--Private Methods
        private Voice getFreeVoice()
        {
            if (freeVoices.Count == 0)
                return null;
            return freeVoices.Pop();
        }
        private Voice getUsedVoice(NoteRegistryKey r)
        {
            List<Voice> voicelist;
            Voice voice;
            if (keyRegistry.TryGetValue(r, out voicelist))
            {
                if (voicelist.Count > 0)
                {
                    voicelist[0].StopImmediately();
                    voice = voicelist[0];
                    voicelist.RemoveAt(0);
                    activeVoices.Remove(voice);
                    return voice;
                }
            }
            return null;
        }
        //private void ConvertBuffer(float[,] from, byte[] to,int length)
        //{
        //    const int bytesPerSample = 2; //again we assume 16 bit audio
        //    int channels = from.GetLength(0);
        //    int bufferSize = from.GetLength(1);

        //    // Make sure the buffer sizes are correct
        //   //UnitySynth
        //   if (!(to.Length == bufferSize * channels * bytesPerSample))
        //        Debug.Log( "Buffer sizes are mismatched.");

        //    for (int i = 0; i < bufferSize; i++)
        //    {
        //        for (int c = 0; c < channels; c++)
        //        {
        //            // Apply master volume
        //            float floatSample = from[c, i] * MainVolume;

        //            // Clamp the value to the [-1.0..1.0] range
        //            floatSample = SynthHelper.Clamp(floatSample, -1.0f, 1.0f);

        //            // Convert it to the 16 bit [short.MinValue..short.MaxValue] range
        //            short shortSample = (short)(floatSample >= 0.0f ? floatSample * short.MaxValue : floatSample * short.MinValue * -1);

        //            // Calculate the right index based on the PCM format of interleaved samples per channel [L-R-L-R]
        //            int index = i * channels * bytesPerSample + c * bytesPerSample;

        //            // Store the 16 bit sample as two consecutive 8 bit values in the buffer with regard to endian-ness
        //            if (!BitConverter.IsLittleEndian)
        //            {
        //                to[index] = (byte)(shortSample >> 8);
        //                to[index + 1] = (byte)shortSample;
        //            }
        //            else
        //            {
        //                to[index] = (byte)shortSample;
        //                to[index + 1] = (byte)(shortSample >> 8);
        //            }
        //        }
        //    }
        //}

        //UnitySynth
        private void ConvertBuffer(float[,] from, float[] to,int length)
        {
            //const int bytesPerSample = 2; //again we assume 16 bit audio
            int channels = from.GetLength(0);
            //int bufferSize = from.GetLength(1);
            int sampleIndex = 0;
            //UnitySynth
            //if (!(to.Length == bufferSize * channels * bytesPerSample))
            //    Debug.Log("Buffer sizes are mismatched.");

            for (int i = 0; i < length; i++)
            {
                for (int c = 0; c < channels; c++)
                {
                    // Apply master volume
                    float floatSample = from[c, i] * MainVolume;
                    // Clamp the value to the [-1.0..1.0] range
                    floatSample = SynthHelper.Clamp(floatSample, -1.0f, 1.0f);
                    to[i] = floatSample;
                }
            }
        }

        private void FillWorkingBuffer(int length)
        {
            // Call Process on all active voices
            LinkedListNode<Voice> node;
            LinkedListNode<Voice> delnode;
            if (seq != null && seq.isPlaying)//Use sequencer
            {
                //MidiSequencerEvent seqEvent = seq.Process(samplesperBuffer);
                MidiSequencerEvent seqEvent = seq.Process(length);
                if (seqEvent == null)
                    return;
                int oldtime = 0;
                int waitTime = 0;
                for (int x = 0; x < seqEvent.Events.Count; x++)
                {
                    waitTime = ((int)seqEvent.Events[x].deltaTime - seq.SampleTime) - oldtime;
                    if (waitTime != 0)
                    {
                        node = activeVoices.First;
                        while (node != null)
                        {
                            if (oldtime < 0 || waitTime < 0)
                                throw new Exception("dd");
                            node.Value.Process(sampleBuffer, oldtime, oldtime + waitTime);
                            if (node.Value.isInUse == false)
                            {
                                delnode = node;
                                node = node.Next;
                                freeVoices.Push(delnode.Value);
                                activeVoices.Remove(delnode);
                            }
                            else
                            {
                                node = node.Next;
                            }
                        }
                    }
                    oldtime = oldtime + waitTime;
                    //Now process the event
                    seq.ProcessMidiEvent(seqEvent.Events[x]);
                }
                //make sure to finish the processing to the end of the buffer
                if (oldtime < length)
                {
                    node = activeVoices.First;
                    while (node != null)
                    {
                        node.Value.Process(sampleBuffer, oldtime, length);
                        if (node.Value.isInUse == false)
                        {
                            delnode = node;
                            node = node.Next;
                            freeVoices.Push(delnode.Value);
                            activeVoices.Remove(delnode);
                        }
                        else
                        {
                            node = node.Next;
                        }
                    }
                }
                //increment our sample count
                seq.IncrementSampleCounter(length);
            }
            else //Manual mode
            {
                node = activeVoices.First;
                while (node != null)
                {
                    //Process buffer with no interrupt for events
                    node.Value.Process(sampleBuffer, 0, length);
                    if (node.Value.isInUse == false)
                    {
                        delnode = node;
                        node = node.Next;
                        freeVoices.Push(delnode.Value);
                        activeVoices.Remove(delnode);
                    }
                    else
                    {
                        node = node.Next;
                    }
                }
            }
        }
        private void ClearWorkingBuffer(int length)
        {
            //Array.Clear(sampleBuffer, 0, audioChannels * samplesperBuffer);
            Array.Clear(sampleBuffer, 0, audioChannels * length);
        }
        private void setupSynth()
        {
            //checks
            if (sampleRate < 8000 || sampleRate > 48000)
            {
                sampleRate = 44100;
                this.samplesperBuffer = (sampleRate / 1000) * 50;
                //UnitySynth
                Debug.Log("-----> Invalid Sample Rate! Changed to---->" + sampleRate);
                Debug.Log("-----> Invalid Buffer Size! Changed to---->" + 50 + "ms");
            }
            if (polyphony < 1 || polyphony > 500)
            {
                polyphony = 40;
                Debug.Log("-----> Invalid Max Poly! Changed to---->" + polyphony);
            }
            if (maxnotepoly < 1 || maxnotepoly > polyphony)
            {
                maxnotepoly = 2;
                Debug.Log("-----> Invalid Max Note Poly! Changed to---->" + maxnotepoly);
            }
            if (samplesperBuffer < 100 || samplesperBuffer > 500000)
            {
                this.samplesperBuffer = (int)((sampleRate / 1000.0) * 50.0);
                Debug.Log("-----> Invalid Buffer Size! Changed to---->" + 50 + "ms");
            }
            if (audioChannels < 1 || audioChannels > 2)
            {
                audioChannels = 1;
                Debug.Log("-----> Invalid Audio Channels! Changed to---->" + audioChannels);
            }
            //initialize variables
            sampleBuffer = new float[audioChannels, samplesperBuffer];
            rawBufferLength = audioChannels * samplesperBuffer * 2; //Assuming 16 bit data
            // Create voice structures
            voicePool = new Voice[polyphony];
            for (int i = 0; i < polyphony; ++i)
                voicePool[i] = new Voice(this);
            freeVoices = new Stack<Voice>(voicePool);
            activeVoices = new LinkedList<Voice>();
            keyRegistry = new Dictionary<NoteRegistryKey, List<Voice>>();
            //Setup Channel Data
            panPositions_ = new float[16];
            volPositions_ = new float[16];
            for (int x = 0; x < volPositions_.Length; x++)
                volPositions_[x] = 1.00f;
            tunePositions_ = new double[16];
            //create effect list
            effects = new List<BasicAudioEffect>();
        }
    }
}
