using System;
using System.Collections.Generic;
using CSharpSynth.Synthesis;
using System.IO;
using UnityEngine;

namespace CSharpSynth.Banks.Fm
{
    public class FMInstrument : Instrument
    {
        //--Variables
        private SynthHelper.WaveFormType baseWaveType;
        private SynthHelper.WaveFormType modWaveType;
        //These parameters are measured in samples...
        private int _attack;
        private int _release;
        private int _decay;
        private int _hold;
        private double start_time;
        private double end_time;
        private bool looping;
        private Envelope env;
        //modulator parameters
        private IFMComponent mamp;
        private IFMComponent mfreq;
        //--Public Properties
        public int Attack
        {
            get { return _attack; }
            set { _attack = value; }
        }
        public int Release
        {
            get { return _release; }
            set { _release = value; }
        }
        public SynthHelper.WaveFormType WaveForm
        {
            get { return baseWaveType; }
            set { baseWaveType = value; }
        }
        //--Public Methods
        public FMInstrument(string fmProgramFile, int sampleRate)
            : base()
        {
            this.SampleRate = sampleRate;
            //Proper calculation of voice states
            _attack = SynthHelper.getSampleFromTime(sampleRate, SynthHelper.DEFAULT_ATTACK);
            _release = SynthHelper.getSampleFromTime(sampleRate, SynthHelper.DEFAULT_RELEASE);
            _decay = SynthHelper.getSampleFromTime(sampleRate, SynthHelper.DEFAULT_DECAY);
            _hold = SynthHelper.getSampleFromTime(sampleRate, SynthHelper.DEFAULT_HOLD);
            //open fm program file
            loadProgramFile(fmProgramFile);
            //set base attribute name
            base.Name = System.IO.Path.GetFileNameWithoutExtension(fmProgramFile);
        }        
        public override bool allSamplesSupportDualChannel()
        {
            return false;
        }
        public override void enforceSampleRate(int sampleRate)
        {
            if (sampleRate != this.SampleRate)
            {
                //Proper calculation of voice states
                _attack = SynthHelper.getSampleFromTime(sampleRate, SynthHelper.DEFAULT_ATTACK);
                _release = SynthHelper.getSampleFromTime(sampleRate, SynthHelper.DEFAULT_RELEASE);
                _decay = SynthHelper.getSampleFromTime(sampleRate, SynthHelper.DEFAULT_DECAY);
                _hold = SynthHelper.getSampleFromTime(sampleRate, SynthHelper.DEFAULT_HOLD);
                this.SampleRate = sampleRate;
            }
        }
        public override int getAttack(int note)
        {
            return _attack;
        }
        public override int getRelease(int note)
        {
            return _release;
        }
        public override int getDecay(int note)
        {
            return _decay;
        }
        public override int getHold(int note)
        {
            return _hold;
        }
        public override float getSampleAtTime(int note, int channel, int synthSampleRate, ref double time)
        {
            //time
            if (time > end_time)
            {
                if (looping)
                    time = start_time;
                else
                {
                    time = end_time;
                    return 0.0f;
                }
            }
            double freq = SynthHelper.NoteToFrequency(note);
            //modulation
            switch (modWaveType)
            {
                case SynthHelper.WaveFormType.Sine:
                    freq = freq + (SynthHelper.Sine(mfreq.doProcess(freq), time) * mamp.doProcess(SynthHelper.DEFAULT_AMPLITUDE));
                    break;
                case SynthHelper.WaveFormType.Sawtooth:
                    freq = freq + (SynthHelper.Sawtooth(mfreq.doProcess(freq), time) * mamp.doProcess(SynthHelper.DEFAULT_AMPLITUDE));
                    break;
                case SynthHelper.WaveFormType.Square:
                    freq = freq + (SynthHelper.Square(mfreq.doProcess(freq), time) * mamp.doProcess(SynthHelper.DEFAULT_AMPLITUDE));
                    break;
                case SynthHelper.WaveFormType.Triangle:
                    freq = freq + (SynthHelper.Triangle(mfreq.doProcess(freq), time) * mamp.doProcess(SynthHelper.DEFAULT_AMPLITUDE));
                    break;
                case SynthHelper.WaveFormType.WhiteNoise:
                    freq = freq + (SynthHelper.WhiteNoise(0, time) * mamp.doProcess(SynthHelper.DEFAULT_AMPLITUDE));
                    break;
                default:
                    break;
            }
            //carrier
            switch (baseWaveType)
            {
                case SynthHelper.WaveFormType.Sine:
                    return SynthHelper.Sine(freq, time) * env.doProcess(time);
                case SynthHelper.WaveFormType.Sawtooth:
                    return SynthHelper.Sawtooth(freq, time) * env.doProcess(time);
                case SynthHelper.WaveFormType.Square:
                    return SynthHelper.Square(freq, time) * env.doProcess(time);
                case SynthHelper.WaveFormType.Triangle:
                    return SynthHelper.Triangle(freq, time) * env.doProcess(time);
                case SynthHelper.WaveFormType.WhiteNoise:
                    return SynthHelper.WhiteNoise(note, time) * env.doProcess(time);
                default:
                    return 0.0f;
            }
        }       
        private void loadProgramFile(string file)
        {
            //Debug.LogError("dload :" + file);
            //UnitySynth
            //StreamReader reader = new StreamReader(File.Open(file, FileMode.Open));
            //Debug.Log(this.ToString() + " AppDataPath " + Application.dataPath + " Filename: " + file);
            var bytes = Resources.Load<TextAsset>(file).bytes;
            StreamReader reader = new StreamReader(new System.IO.MemoryStream(bytes));
            //StreamReader reader = new StreamReader(Application.dataPath + "/Resources/" + file);

            if (!reader.ReadLine().Trim().ToUpper().Equals("[FM INSTRUMENT]"))
            {
                reader.Close();
                throw new Exception("Invalid Program file: Incorrect Header!");
            }
            string[] args = reader.ReadLine().Split(new string[] { "|" }, StringSplitOptions.None);
            if (args.Length < 4)
            {
                reader.Close();
                throw new Exception("Invalid Program file: Parameters are missing");
            }
            this.baseWaveType = SynthHelper.getTypeFromString(args[0]);
            this.modWaveType = SynthHelper.getTypeFromString(args[1]);
            this.mfreq = getOpsAndValues(args[2], true);
            this.mamp = getOpsAndValues(args[3], false);
            args = reader.ReadLine().Split(new string[] { "|" }, StringSplitOptions.None);
            if (args.Length < 3)
            {
                reader.Close();
                throw new Exception("Invalid Program file: Parameters are missing");
            }
            if (int.Parse(args[0]) == 0)
                looping = true;
            start_time = double.Parse(args[1]);
            end_time = double.Parse(args[2]);
            args = reader.ReadLine().Split(new string[] { "|" }, StringSplitOptions.None);
            if (args.Length < 3)
            {
                reader.Close();
                throw new Exception("Invalid Program file: Parameters are missing");
            }
            switch (args[0].ToLower().Trim())
            {
                case "fadein":
                    env = Envelope.CreateBasicFadeIn(double.Parse(args[2]));
                    break;
                case "fadeout":
                    env = Envelope.CreateBasicFadeOut(double.Parse(args[2]));
                    break;
                case "fadein&out":
                    double p = double.Parse(args[2]) / 2.0;
                    env = Envelope.CreateBasicFadeInAndOut(p, p);
                    break;
                default:
                    env = Envelope.CreateBasicConstant();
                    break;
            }
            env.Peak = double.Parse(args[1]);
            reader.Close();
        }
        private IFMComponent getOpsAndValues(string arg, bool isFrequencyFunction)
        {
            arg = arg + "    ";
            char[] chars = arg.ToCharArray();
            List<byte> opList = new List<byte>();
            List<double> valueList = new List<double>();
            string start = arg.Substring(0, 4).ToLower();
            if (isFrequencyFunction)
            {
                if (!start.Contains("freq"))
                {//if "freq" isnt used then we make sure the value passed in is negated by *0;
                    opList.Add(0);
                    valueList.Add(0);
                }
            }
            else
            {
                if (!start.Contains("amp"))
                {//if "amp" isnt used then we make sure the value passed in is negated by *0;
                    opList.Add(0);
                    valueList.Add(0);
                }
            }
            bool opOcurred = false;
            bool neg = false;
            for (int x = 0; x < arg.Length; x++)
            {
                switch (chars[x])
                {
                    case '*':
                        if (opOcurred == false)
                        {
                            opList.Add(0);
                            opOcurred = true;
                        }
                        break;
                    case '/':
                        if (opOcurred == false)
                        {
                            opList.Add(1);
                            opOcurred = true;
                        }
                        break;
                    case '+':
                        if (opOcurred == false)
                        {
                            opList.Add(2);
                            opOcurred = true;
                        }
                        break;
                    case '-':
                        if (opOcurred == true)
                            neg = !neg;
                        else
                        {
                            opList.Add(3);
                            opOcurred = true;
                        }
                        break;
                    default:
                        string number = "";
                        while (Char.IsDigit(chars[x]) || chars[x] == '.')
                        {
                            number = number + chars[x];
                            x++;
                            if (x >= chars.Length)
                                break;
                        }
                        if (number.Length > 0)
                        {
                            x--;
                            opOcurred = false;
                            if (neg)
                                number = "-" + number;
                            neg = false;
                            valueList.Add(double.Parse(number));
                        }
                        break;
                }
            }
            while (opList.Count < valueList.Count)
                opList.Add(2);
            if (isFrequencyFunction)
                return new ModulatorFrequencyFunction(opList.ToArray(), valueList.ToArray());
            else
                return new ModulatorAmplitudeFunction(opList.ToArray(), valueList.ToArray());
        }
        //--Private Classes
        private class ModulatorFrequencyFunction : IFMComponent
        {
            private byte[] ops; //0 = "*", 1 = "/", 2 = "+", 3 = "-"
            private double[] values;
            public ModulatorFrequencyFunction(byte[] ops, double[] values)
            {
                if (ops.Length != values.Length)
                    throw new Exception("Invalid FM frequency function.");
                this.ops = ops;
                this.values = values;
            }
            public double doProcess(double value)
            {
                for (int x = 0; x < ops.Length; x++)
                {
                    switch (ops[x])
                    {
                        case 0:
                            value = value * values[x];
                            break;
                        case 1:
                            value = value / values[x];
                            break;
                        case 2:
                            value = value + values[x];
                            break;
                        case 3:
                            value = value - values[x];
                            break;
                    }
                }
                return value;
            }
        }
        private class ModulatorAmplitudeFunction : IFMComponent
        {
            private byte[] ops; //0 = "*", 1 = "/", 2 = "+", 3 = "-"
            private double[] values;
            public ModulatorAmplitudeFunction(byte[] ops, double[] values)
            {
                if (ops.Length != values.Length)
                    throw new Exception("Invalid FM Amplitude function.");
                this.ops = ops;
                this.values = values;
            }
            public double doProcess(double value)
            {
                for (int x = 0; x < ops.Length; x++)
                {
                    switch (ops[x])
                    {
                        case 0:
                            value = value * values[x];
                            break;
                        case 1:
                            value = value / values[x];
                            break;
                        case 2:
                            value = value + values[x];
                            break;
                        case 3:
                            value = value - values[x];
                            break;
                    }
                }
                return value;
            }
        }
    }
}
