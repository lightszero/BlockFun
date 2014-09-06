using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CSharpSynth.Wave
{
    public class WaveFileReader
    {
        //--Variables
        private BinaryReader BR;
        //--Public Methods
        public WaveFileReader(string filename)
        {
            //UnitySynth
            //if (Path.GetExtension(filename).ToLower() != ".wav" || File.Exists(filename) == false)
            //    throw new IOException("Invalid wave file!");
            //BR = new System.IO.BinaryReader(System.IO.File.OpenRead(filename));

            //NOTE: WAVE FILES NEED .bytes appended. See http://unity3d.com/support/documentation/Components/class-TextAsset.html
            TextAsset fileName = Resources.Load(filename) as TextAsset;
            //Debug.Log(this.ToString() + " AppDataPath " + Application.dataPath + " Filename: " + filename + " asset.bytes.Length " + asset.bytes.Length.ToString());
            Stream waveFileStream = new MemoryStream(fileName.bytes);
            //Debug.Log(filename);
            BR = new BinaryReader(waveFileStream);
        }
        public IChunk[] ReadAllChunks()
        {
            List<IChunk> CList = new List<IChunk>();
            while (BR.BaseStream.Position < BR.BaseStream.Length)
            {
                IChunk tchk = ReadNextChunk();
                if (tchk != null)
                    CList.Add(tchk);
            }
            return CList.ToArray();
        }
        public IChunk ReadNextChunk()
        {
            if (BR.BaseStream.Position + 4 >= BR.BaseStream.Length)
            {
                BR.BaseStream.Position += 4;
                return null;
            }
            string chkid = (System.Text.UTF8Encoding.UTF8.GetString(BR.ReadBytes(4), 0, 4)).ToLower();
            switch (chkid)
            {
                case "riff":
                    MasterChunk mc = new MasterChunk();
                    mc.chkID = new char[] { 'R', 'I', 'F', 'F' };
                    mc.chksize = BitConverter.ToInt32(BR.ReadBytes(4), 0);
                    mc.WAVEID = BR.ReadChars(4);
                    return mc;
                case "fact":
                    FactChunk fc = new FactChunk();
                    fc.chkID = new char[] { 'f', 'a', 'c', 't' };
                    fc.chksize = BitConverter.ToInt32(BR.ReadBytes(4), 0);
                    fc.dwSampleLength = BitConverter.ToInt32(BR.ReadBytes(4), 0);
                    return fc;
                case "data":
                    DataChunk dc = new DataChunk();
                    dc.chkID = new char[] { 'd', 'a', 't', 'a' };
                    dc.chksize = BitConverter.ToInt32(BR.ReadBytes(4), 0);
                    if (dc.chksize % 2 == 0)
                        dc.pad = 0;
                    else
                        dc.pad = 1;
                    dc.sampled_data = BR.ReadBytes(dc.chksize);
                    return dc;
                case "fmt ":
                    FormatChunk fc2 = new FormatChunk();
                    fc2.chkID = new char[] { 'f', 'm', 't', ' ' };
                    fc2.chksize = BitConverter.ToInt32(BR.ReadBytes(4), 0);
                    fc2.wFormatTag = BitConverter.ToInt16(BR.ReadBytes(2), 0);
                    fc2.nChannels = BitConverter.ToInt16(BR.ReadBytes(2), 0);
                    fc2.nSamplesPerSec = BitConverter.ToInt32(BR.ReadBytes(4), 0);
                    fc2.nAvgBytesPerSec = BitConverter.ToInt32(BR.ReadBytes(4), 0);
                    fc2.nBlockAlign = BitConverter.ToInt16(BR.ReadBytes(2), 0);
                    fc2.wBitsPerSample = BitConverter.ToInt16(BR.ReadBytes(2), 0);
                    if (fc2.wFormatTag != (short)WaveHelper.Format_Code.WAVE_FORMAT_PCM)
                    {
                        fc2.cbSize = BitConverter.ToInt16(BR.ReadBytes(2), 0);
                    }
                    if ((int)fc2.wFormatTag == (int)WaveHelper.Format_Code.WAVE_FORMAT_EXTENSIBLE)
                    {
                        fc2.wValidBitsPerSample = BitConverter.ToInt16(BR.ReadBytes(2), 0);
                        fc2.dwChannelMask = BitConverter.ToInt32(BR.ReadBytes(4), 0);
                        fc2.SubFormat = BR.ReadChars(16);
                    }
                    return fc2;
                default:
                    break;
            }
            return null;
        }
        public WaveFile ReadWaveFile()
        {
            return new WaveFile(ReadAllChunks());
        }
        public void Close()
        {
            BR.Close();
            //UnitySynth
            //BR.Dispose();
        }
    }
}
