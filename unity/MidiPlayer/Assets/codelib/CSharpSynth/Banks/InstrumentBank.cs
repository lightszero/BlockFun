using System;
using System.Collections.Generic;
using CSharpSynth.Wave;
using CSharpSynth.Synthesis;
using System.IO;
using CSharpSynth.Banks.Analog;
using CSharpSynth.Banks.Fm;
using CSharpSynth.Banks.Sfz;
using UnityEngine;

namespace CSharpSynth.Banks
{
    public class InstrumentBank
    {
        //--Variables
        private List<Instrument> Bank = new List<Instrument>();
        private List<Instrument> DrumBank = new List<Instrument>();
        private List<string> SampleName = new List<string>();
        private List<Sample> Samples = new List<Sample>();
        private string lastbankpath = "";
        private int SampleRate_;
        private int SampleMemUse;
        //--Static Variables
        public static Sample nullSample = new Sample(SynthHelper.DEFAULT_SAMPLERATE);
        //--Public Methods
        public InstrumentBank(int sampleRate, string bankfile)
        {
            this.SampleRate_ = sampleRate;
            loadBank(bankfile);
            reCalculateMemoryUsage();
        }
        public InstrumentBank(int sampleRate, string bankfile, byte[] Programs, byte[] DrumPrograms)
        {
            this.SampleRate_ = sampleRate;
            lastbankpath = bankfile;
            loadBank(Programs, DrumPrograms);
            reCalculateMemoryUsage();
        }
        public void loadBank(string bankfile)
        {
            Clear();
            Bank.Capacity = BankManager.DEFAULT_BANK_SIZE;
            DrumBank.Capacity = BankManager.DEFAULT_DRUMBANK_SIZE;
            for (int x = 0; x < BankManager.DEFAULT_BANK_SIZE; x++)
                Bank.Add(null);
            for (int x = 0; x < BankManager.DEFAULT_DRUMBANK_SIZE; x++)
                DrumBank.Add(null);
            //UnitySynth
            //loadStream(File.Open(bankfile, FileMode.Open), Path.GetDirectoryName(bankfile) + "\\", null, null);
            TextAsset bankFile = Resources.Load(bankfile) as TextAsset;
            Debug.Log("loadBank(string bankfile) " + bankfile);
            Stream bankStream = new MemoryStream(bankFile.bytes);
            this.loadStream(bankStream, Path.GetDirectoryName(bankfile) + "/", null, null);
            
            this.lastbankpath = bankfile;
        }
        public void loadBank(byte[] Programs, byte[] DrumPrograms)
        {
            if (File.Exists(lastbankpath) == false)
                return;
            Clear();
            Bank.Capacity = BankManager.DEFAULT_BANK_SIZE;
            DrumBank.Capacity = BankManager.DEFAULT_DRUMBANK_SIZE;
            for (int x = 0; x < BankManager.DEFAULT_BANK_SIZE; x++)
                Bank.Add(null);
            for (int x = 0; x < BankManager.DEFAULT_DRUMBANK_SIZE; x++)
                DrumBank.Add(null);
            //UnitySynth
            //loadStream(File.Open(lastbankpath, FileMode.Open), Path.GetDirectoryName(lastbankpath) + "\\", Programs, DrumPrograms);
            TextAsset lastBankPath = Resources.Load(lastbankpath) as TextAsset;
            Debug.Log("loadBank(byte[] Programs, byte[] DrumPrograms) " + lastbankpath);
            Stream bankStream = new MemoryStream(lastBankPath.bytes);
            this.loadStream(bankStream, Path.GetDirectoryName(lastbankpath) + "/", Programs, DrumPrograms);
        }
        public void loadStream(Stream bankStream, string directory, byte[] Programs, byte[] DrumPrograms)
        {
            StreamReader reader = new StreamReader(bankStream);
            List<string> text = new List<string>();
            while (reader.Peek() > -1)
                text.Add(reader.ReadLine());
            reader.Close();
            bankStream.Close();
            if (text[0].Trim() != "[BankFile]")
                throw new Exception("Not a valid BankFile!");
            for (int x = 1; x < text.Count; x++)
            {//Load each instrument, banks can have mixed instruments!
                string[] split = text[x].Split(new string[] { "/" }, StringSplitOptions.None);
                switch(split[1].Trim().ToLower())
                {
                    case "analog":
                        loadAnalog(split, Programs, DrumPrograms);
                        break;
                    case "fm":
                        loadFm(split, directory, Programs, DrumPrograms);
                        break;
                    case "sfz":
                        loadSfz(split, directory, Programs, DrumPrograms);
                        break;
                }
            }
        }
        public void addInstrument(Instrument inst, bool isDrum)
        {
            if (isDrum == false)
            {
                if (Bank.Contains(inst) == false)
                {
                    //Resample if necessary
                    if (SampleRate_ > 0)
                        inst.enforceSampleRate(SampleRate_);
                    if (inst.SampleList != null)
                    {
                        for (int x = 0; x < inst.SampleList.Length; x++)
                        {//If the instrument contains any new samples get their memory use and add them.
                            if (SampleName.Contains(inst.SampleList[x].Name) == false)
                            {
                                SampleMemUse = SampleMemUse + inst.SampleList[x].getMemoryUseage();
                                SampleName.Add(inst.SampleList[x].Name);
                                Samples.Add(inst.SampleList[x]);
                            }
                        }
                    }
                }
                Bank.Add(inst);
            }
            else
            {
                if (DrumBank.Contains(inst) == false)
                {
                    //Resample if necessary
                    if (SampleRate_ > 0)
                        inst.enforceSampleRate(SampleRate_);
                    if (inst.SampleList != null)
                    {
                        for (int x = 0; x < inst.SampleList.Length; x++)
                        {
                            SampleMemUse = SampleMemUse + inst.SampleList[x].getMemoryUseage();
                            if (SampleName.Contains(inst.SampleList[x].Name) == false)
                            {
                                SampleName.Add(inst.SampleList[x].Name);
                                Samples.Add(inst.SampleList[x]);
                            }
                        }
                    }
                }
                DrumBank.Add(inst);
            }
        }
        public Instrument getInstrument(int index, bool isDrum)
        {
            if (isDrum == false)
                return Bank[index];
            else
                return DrumBank[index];
        }
        public List<Instrument> getInstruments(bool isDrum)
        {
            if (isDrum == false)
                return Bank;
            else
                return DrumBank;
        }
        public void removeInstrument(int index, bool isDrum)
        {//Does not delete the index location so the other instruments keep their locations
            if (isDrum == true)
                DrumBank[index] = null;
            else
                Bank[index] = null;
        }
        public void deleteUnusedSamples()
        {//Now that the InstrumentBank keeps a reference to it's samples
            //you must call this method after you remove instruments
            //to make sure their samples get deleted as well.
            //You don't have to use this after calling Clear() however.

            //Delete and Rebuild Sample List
            Samples.Clear();
            SampleName.Clear();
            for (int x = 0; x < Bank.Count; x++)
            {
                if (Bank[x] != null)
                {
                    Sample[] samps = Bank[x].SampleList;
                    for (int x2 = 0; x2 < samps.Length; x2++)
                    {
                        if (SampleName.Contains(samps[x2].Name) == false)
                        {
                            SampleName.Add(samps[x2].Name);
                            Samples.Add(samps[x2]);
                        }
                    }
                }
            }
            for (int x = 0; x < DrumBank.Count; x++)
            {
                if (DrumBank[x] != null)
                {
                    Sample[] samps = DrumBank[x].SampleList;
                    for (int x2 = 0; x2 < samps.Length; x2++)
                    {
                        if (SampleName.Contains(samps[x2].Name) == false)
                        {
                            SampleName.Add(samps[x2].Name);
                            Samples.Add(samps[x2]);
                        }
                    }
                }
            }
            reCalculateMemoryUsage();
        }
        public void Clear()
        {
            Bank.Clear();
            DrumBank.Clear();
            SampleName.Clear();
            Samples.Clear();
            SampleMemUse = 0;
        }
        //--Public Properties
        public int InstrumentCount
        {
            get { return Bank.Count; }
        }
        public string BankPath
        {
            get { return lastbankpath; }
            set { lastbankpath = value; }
        }
        public static Sample DummySample
        {
            get { return nullSample; }
        }
        public List<string> SampleNameList
        {
            get { return SampleName; }
        }
        public List<Sample> SampleList
        {
            get { return Samples; }
        }
        public int DrumCount
        {
            get { return DrumBank.Count; }
        }
        public int MemoryUsage
        {
            get { return SampleMemUse; }
        }
        public int SampleRate
        {
            get { return SampleRate_; }
            set { SampleRate_ = value; }
        }
        //--Private Methods
        private void loadAnalog(string[] args, byte[] Programs, byte[] DrumPrograms)
        {
            bool ISdrum = args[4] == "d" ? true : false;
            int start = int.Parse(args[2]);
            int end = int.Parse(args[3]);
            List<int> Indices = new List<int>();

            if (ISdrum == false)
            {
                if (Programs == null)
                {
                    for (int i = start; i <= end; i++)
                    {
                        Indices.Add(i);
                    }
                }
                else
                {
                    for (int x2 = 0; x2 < Programs.Length; x2++)
                    {
                        if (Programs[x2] >= start && Programs[x2] <= end)
                        {
                            Indices.Add(Programs[x2]);
                        }
                    }
                }
            }
            else
            {
                if (DrumPrograms == null)
                {
                    for (int i = start; i <= end; i++)
                    {
                        Indices.Add(i);
                    }
                }
                else
                {
                    for (int x2 = 0; x2 < DrumPrograms.Length; x2++)
                    {
                        if (DrumPrograms[x2] >= start && Programs[x2] <= end)
                        {
                            Indices.Add(DrumPrograms[x2]);
                        }
                    }
                }
            }

            if (Indices.Count > 0)
            {
                Instrument inst;
                inst = new AnalogInstrument(SynthHelper.getTypeFromString(args[0]), SampleRate_);
                //Resample if necessary
                if (SampleRate_ > 0)
                    inst.enforceSampleRate(SampleRate_);
                //Loop through where to add the instruments
                for (int i = 0; i < Indices.Count; i++)
                {
                    //Decide which bank to add too
                    if (ISdrum == true)
                        DrumBank[Indices[i]] = inst;
                    else
                        Bank[Indices[i]] = inst;
                }
            }
        }
        private void loadFm(string[] args, string bankpath, byte[] Programs, byte[] DrumPrograms)
        {
            bool ISdrum = args[4] == "d" ? true : false;
            int start = int.Parse(args[2]);
            int end = int.Parse(args[3]);
            List<int> Indices = new List<int>();

            if (ISdrum == false)
            {
                if (Programs == null)
                {
                    for (int i = start; i <= end; i++)
                    {
                        Indices.Add(i);
                    }
                }
                else
                {
                    for (int x2 = 0; x2 < Programs.Length; x2++)
                    {
                        if (Programs[x2] >= start && Programs[x2] <= end)
                        {
                            Indices.Add(Programs[x2]);
                        }
                    }
                }
            }
            else
            {
                if (DrumPrograms == null)
                {
                    for (int i = start; i <= end; i++)
                    {
                        Indices.Add(i);
                    }
                }
                else
                {
                    for (int x2 = 0; x2 < DrumPrograms.Length; x2++)
                    {
                        if (DrumPrograms[x2] >= start && Programs[x2] <= end)
                        {
                            Indices.Add(DrumPrograms[x2]);
                        }
                    }
                }
            }

            if (Indices.Count > 0)
            {
                Instrument inst;
                inst = new FMInstrument(bankpath + args[0] + ".prg", SampleRate_);
                //Resample if necessary
                if (SampleRate_ > 0)
                    inst.enforceSampleRate(SampleRate_);
                //Loop through where to add the instruments
                for (int i = 0; i < Indices.Count; i++)
                {
                    //Decide which bank to add too
                    if (ISdrum == true)
                        DrumBank[Indices[i]] = inst;
                    else
                        Bank[Indices[i]] = inst;
                }
            }
        }
        private void loadSfz(string[] args, string bankpath, byte[] Programs, byte[] DrumPrograms)
        {
            bool ISdrum = args[4] == "d" ? true : false;
            int start = int.Parse(args[2]);
            int end = int.Parse(args[3]);
            List<int> Indices = new List<int>();

            if (ISdrum == false)
            {
                if (Programs == null)
                {
                    for (int i = start; i <= end; i++)
                    {
                        Indices.Add(i);
                    }
                }
                else
                {
                    for (int x2 = 0; x2 < Programs.Length; x2++)
                    {
                        if (Programs[x2] >= start && Programs[x2] <= end)
                        {
                            Indices.Add(Programs[x2]);
                        }
                    }
                }
            }
            else
            {
                if (DrumPrograms == null)
                {
                    for (int i = start; i <= end; i++)
                    {
                        Indices.Add(i);
                    }
                }
                else
                {
                    for (int x2 = 0; x2 < DrumPrograms.Length; x2++)
                    {
                        if (DrumPrograms[x2] >= start && Programs[x2] <= end)
                        {
                            Indices.Add(DrumPrograms[x2]);
                        }
                    }
                }
            }

            if (Indices.Count > 0)
            {
                Instrument inst;
                inst = new SfzInstrument(bankpath + args[0] + ".sfz", SampleRate_, this);
                //Resample if necessary
                if (SampleRate_ > 0)
                    inst.enforceSampleRate(SampleRate_);
                //Loop through where to add the instruments
                for (int i = 0; i < Indices.Count; i++)
                {
                    //Decide which bank to add too
                    if (ISdrum == true)
                        DrumBank[Indices[i]] = inst;
                    else
                        Bank[Indices[i]] = inst;
                }
            }
        }
        private void reCalculateMemoryUsage()
        {
            SampleMemUse = 0;
            for (int x = 0; x < Samples.Count; x++)
                SampleMemUse += Samples[x].getMemoryUseage();
        }
    }
}
