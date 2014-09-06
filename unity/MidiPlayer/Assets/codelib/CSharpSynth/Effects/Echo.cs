using System;
using CSharpSynth.Synthesis;

namespace CSharpSynth.Effects
{
    public class Echo : BasicAudioEffect
    {
        //--Variables
        private int channels;
        private int secondarybufferlen;
        private int secondaryposition;
        private float decay;
        //--Public Properties
        public float Decay
        {
            get { return decay; }
            set { this.decay = SynthHelper.Clamp(value, 0.0f, 1.0f); }
        }
        //--Public Methods
        /// <summary>
        /// A simple echo effect.
        /// </summary>
        /// <param name="synth">A constructed synthesizer instance.</param>
        /// <param name="delaytime">Echo delay in seconds.</param>
        /// <param name="decay">Controls the volume of the echo.</param>
        public Echo(StreamSynthesizer synth, float delaytime, float decay)
            : base()
        {
            if (delaytime <= 0.0f)
                throw new ArgumentException("delay time must be positive non-zero for echo effect.");
            this.decay = SynthHelper.Clamp(decay, 0.0f, 1.0f);
            this.EffectBuffer = new float[synth.Channels, SynthHelper.getSampleFromTime(synth.SampleRate, delaytime)];
            channels = this.EffectBuffer.GetLength(0);
            secondarybufferlen = this.EffectBuffer.GetLength(1);
        }
        public void resetEcho()
        {
            secondaryposition = 0;
            Array.Clear(this.EffectBuffer, 0, secondarybufferlen * channels);
        }
        public override void doEffect(float[,] inputBuffer,int length)
        {
            //int primarybufferlen = inputBuffer.GetLength(1);
            int primarybufferlen = length;
            for (int counter = 0; counter < primarybufferlen; counter++)
            {
                for (int x = 0; x < channels; x++)
                {
                    float mixed = inputBuffer[x, counter] + decay * this.EffectBuffer[x, secondaryposition];
                    this.EffectBuffer[x, secondaryposition] = mixed;
                    inputBuffer[x, counter] = mixed;
                }
                secondaryposition++;
                if (secondaryposition == secondarybufferlen)
                {
                    secondaryposition = 0;
                }
            }
        }
    }
}
