using System;

namespace CSharpSynth.Synthesis
{
    public static class SynthHelper
    {
        //--Constants
        public const float DEFAULT_ATTACK = .001f;           //gradually brings volume up when note starts.
        public const float DEFAULT_RELEASE = .001f;          //gradually brings volume down when note ends.
        public const float DEFAULT_DECAY = 9999.0f;          //gradually brings volume down during playback.
        public const float DEFAULT_HOLD = .01f;              //controls how long sustain is held after noteOff.
        public const int DEFAULT_SAMPLERATE = 44100;
        public const double STARTING_FREQUENCY = 8.1757989156;
        public const float DEFAULT_AMPLITUDE = .25f;
        public enum WaveFormType { Sine = 0, Sawtooth = 1, Square = 2, Triangle = 3, WhiteNoise = 4 }
        //--Private Static
        private static Random rnd = new Random();
        //--Public Static Methods
        public static double getRandom()
        {
            return rnd.NextDouble();
        }
        public static int getSampleFromTime(int sampleRate, float timeInSeconds)
        {
            return (int)(sampleRate * timeInSeconds);
        }
        public static float getTimeFromSample(int sampleRate, int Sample)
        {
            return Sample / (float)sampleRate;
        }
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            else if (value > max)
                return max;
            return value;
        }
        public static double Clamp(double value, double min, double max)
        {
            if (value < min)
                return min;
            else if (value > max)
                return max;
            return value;
        }
        //--WaveForm Methods
        public static float Sine(double frequency, double time)
        {
            return (float)Math.Sin(frequency * time * 2.0 * Math.PI);
        }
        public static float Square(double frequency, double time)
        {
            return Sine(frequency, time) >= 0.0f ? 1.0f : -1.0f;
        }
        public static float Sawtooth(double frequency, double time)
        {
            return (float)(2.0 * (time * frequency - Math.Floor(time * frequency + 0.5)));
        }
        public static float Triangle(double frequency, double time)
        {
            return Math.Abs(Sawtooth(frequency, time)) * 2.0f - 1.0f;
        }
        public static float WhiteNoise(int note, double time)
        {
            return (float)(SynthHelper.getRandom() - (SynthHelper.getRandom()));
        }
        public static double NoteToFrequency(double note)
        {
            return Math.Pow(2.0, note / 12.0) * STARTING_FREQUENCY;
        }
        public static WaveFormType getTypeFromString(string wavetype)
        {
            switch (wavetype.Trim().ToLower())
            {
                case "sine":
                    return SynthHelper.WaveFormType.Sine;
                case "sawtooth":
                    return SynthHelper.WaveFormType.Sawtooth;
                case "square":
                    return SynthHelper.WaveFormType.Square;
                case "triangle":
                    return SynthHelper.WaveFormType.Triangle;
                case "whitenoise":
                    return SynthHelper.WaveFormType.WhiteNoise;
                default:
                    return SynthHelper.WaveFormType.Square;
            }
        }
    }
}
