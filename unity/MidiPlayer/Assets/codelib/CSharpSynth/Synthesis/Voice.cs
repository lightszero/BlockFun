using System;
using CSharpSynth.Banks;

namespace CSharpSynth.Synthesis
{
    public class Voice
    {
        //--Variables
        private Instrument inst;
        //voice parameters
        private int note;
        private int velocity;
        private int attack;
        private int release;
        private int hold;
        private int decay;
        private int channel;
        private float pan;
        private float rightpan;
        private float leftpan;
        private double variableSampleRate;
        //counters and modifiers
        private bool inUse;
        private VoiceState state;
        private StreamSynthesizer synth;
        private double time;
        private float fadeMultiplier;
        private int fadeCounter;
        private int decayCounter;
        private float gainControl = .3f;
        //--Enum
        private enum VoiceState { None, Attack, Sustain, Hold, Release }
        //--Public Methods
        public Voice(StreamSynthesizer synth)
        {
            resetVoice();
            this.synth = synth;
            this.inst = null;
        }
        public Voice(StreamSynthesizer synth, Instrument inst)
        {
            resetVoice();
            this.synth = synth;
            setInstrument(inst);
        }
        public void setInstrument(Instrument inst)
        {
            if (this.inst != inst)
                this.inst = inst;
        }
        public Instrument getInstrument()
        {
            return inst;
        }
        public void Start(int channel, int note, int velocity)
        {
            this.note = note;
            this.velocity = velocity;
            this.channel = channel;
            time = 0.0;
            fadeMultiplier = 1.0f;
            decayCounter = 0;
            fadeCounter = 0;

            //Set note parameters in samples
            attack = inst.getAttack(note);
            release = inst.getRelease(note);
            hold = inst.getHold(note);
            decay = inst.getDecay(note);

            //Set counters and initial state
            decayCounter = decay;
            if (attack == 0)
                state = VoiceState.Sustain;
            else
            {
                state = VoiceState.Attack;
                fadeCounter = attack;
            }
            inUse = true;
        }
        public void Stop()
        {
            if (hold == 0)
            {
                if (release == 0)
                {
                    state = VoiceState.None;
                    inUse = false;
                }
                else
                {
                    state = VoiceState.Release;
                    fadeCounter = release;
                }
            }
            else
            {
                state = VoiceState.Hold;
                fadeCounter = hold;
            }
        }
        public void StopImmediately()
        {
            state = VoiceState.None;
            inUse = false;
        }
        public bool isInUse
        {
            get { return inUse; }
        }
        public void setPan(float pan)
        {
            if (pan >= -1.0f && pan <= 1.0f && this.pan != pan)
            {
                this.pan = pan;
                if (pan > 0.0f)
                {
                    rightpan = 1.00f;
                    leftpan = 1.00f - pan;
                }
                else
                {
                    leftpan = 1.0f;
                    rightpan = 1.00f + pan;
                }
            }
        }
        public float getPan()
        {
            return pan;
        }
        public NoteRegistryKey getKey()
        {
            return new NoteRegistryKey((byte)channel, (byte)note);
        }
        public void Process(float[,] workingBuffer, int startIndex, int endIndex)
        {
            if (inUse)
            {
                //quick checks to do before we go through our main loop
                if (synth.Channels == 2 && pan != synth.PanPositions[channel])
                    this.setPan(synth.PanPositions[channel]);
                //set sampleRate for tune
                variableSampleRate = synth.SampleRate * Math.Pow(2.0, (synth.TunePositions[channel] * -1.0) / 12.0);
                //main loop
                for (int i = startIndex; i < endIndex; i++)
                {
                    //manage states and calculate volume level
                    switch (state)
                    {
                        case VoiceState.Attack:
                            fadeCounter--;
                            if (fadeCounter <= 0)
                            {
                                state = VoiceState.Sustain;
                                fadeMultiplier = 1.0f;
                            }
                            else
                            {
                                fadeMultiplier = 1.0f - (fadeCounter / (float)attack);
                            }
                            break;
                        case VoiceState.Sustain:
                            decayCounter--;
                            if (decayCounter <= 0)
                            {
                                state = VoiceState.None;
                                inUse = false;
                                fadeMultiplier = 0.0f;
                            }
                            else
                            {
                                fadeMultiplier = decayCounter / (float)decay;
                            }
                            break;
                        case VoiceState.Hold:
                            fadeCounter--;//not used for volume
                            decayCounter--;
                            if (decayCounter <= 0)
                            {
                                state = VoiceState.None;
                                inUse = false;
                                fadeMultiplier = 0.0f;
                            }
                            else if (fadeCounter <= 0)
                            {
                                state = VoiceState.Release;
                                fadeCounter = release;
                            }
                            else
                            {
                                fadeMultiplier = decayCounter / (float)decay;
                            }
                            break;
                        case VoiceState.Release:
                            fadeCounter--;
                            if (fadeCounter <= 0)
                            {
                                state = VoiceState.None;
                                inUse = false;
                            }
                            else
                            {//Multiply decay with fadeout so volume doesn't suddenly rise when releasing notes
                                fadeMultiplier = (decayCounter / (float)decay) * (fadeCounter / (float)release);
                            }
                            break;
                    }
                    //end of state management
                    //Decide how to sample based on channels available
                    
                    //mono output
                    if (synth.Channels == 1)
                    {
                        float sample = inst.getSampleAtTime(note, 0, synth.SampleRate, ref time);
                        sample = sample * (velocity / 127.0f) * synth.VolPositions[channel];
                        workingBuffer[0, i] += (sample * fadeMultiplier * gainControl);
                    }
                    //mono sample to stereo output
                    else if (synth.Channels == 2 && inst.allSamplesSupportDualChannel() == false)
                    {
                        float sample = inst.getSampleAtTime(note, 0, synth.SampleRate, ref time);
                        sample = sample * (velocity / 127.0f) * synth.VolPositions[channel];

                        workingBuffer[0, i] += (sample * fadeMultiplier * leftpan * gainControl);
                        workingBuffer[1, i] += (sample * fadeMultiplier * rightpan * gainControl);
                    }
                    //both support stereo
                    else
                    {

                    }
                    time += 1.0 / variableSampleRate;
                    //bailout of the loop if there is no reason to continue.
                    if (inUse == false)
                        return;
                }
            }
        }
        //--Private Methods
        private void resetVoice()
        {
            inUse = false;
            state = VoiceState.None;
            note = 0;
            time = 0.0;
            fadeMultiplier = 1.0f;
            decayCounter = 0;
            fadeCounter = 0;
            pan = 0.0f;
            channel = 0;
            rightpan = 1.0f;
            leftpan = 1.0f;
            velocity = 127;
        }
    }
}
