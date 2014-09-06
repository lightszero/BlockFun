using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

namespace CSharpSynth.Midi
{
    public class MidiFile
    {
        //--Variables
        private uint BPM = 120; //beats per minute
        private uint MPQN = 500000; //microseconds per 1/4 note
        private bool sequenceReadyFormat = false;
        private MidiHeader mheader;
        private MidiTrack[] tracks;
        //--Public Properties
        public bool SequencerReady
        {
            get { return sequenceReadyFormat; }
            set { sequenceReadyFormat = value; }
        }
        public MidiTrack[] Tracks
        {
            get { return tracks; }
        }
        public uint BeatsPerMinute
        {
            get { return BPM; }
            set
            {
                BPM = value;
                MPQN = MidiHelper.MicroSecondsPerMinute / BPM;
            }
        }
        public uint MicroSecondsPerQuarterNote
        {
            get { return MPQN; }
            set
            {
                MPQN = value;
                BPM = MidiHelper.MicroSecondsPerMinute / MPQN;
            }
        }
        public MidiHeader MidiHeader
        {
            get { return mheader; }
        }
        //--Public Methods
        public MidiFile(string filename)
        {
            Stream midiStream = null;
            try
            {
                //UnitySynth
                //midiStream = File.Open(filename, FileMode.Open);
                TextAsset midiFileName = Resources.Load(filename) as TextAsset;
                midiStream = new MemoryStream(midiFileName.bytes);
                loadStream(midiStream);
            }
            catch (Exception ex)
            {
                throw new Exception("Midi Failed to Load!", ex);
            }
            finally
            {
                if (midiStream != null)
                    midiStream.Close();
            }
        }
        public List<MidiEvent> getAllMidiEventsofType(MidiHelper.MidiChannelEvent eventChannelType, MidiHelper.MidiMetaEvent eventMetaType)
        {
            List<MidiEvent> matchList = new List<MidiEvent>();
            for (int x = 0; x < tracks.Length; x++)
            {
                for (int i = 0; i < tracks[x].MidiEvents.Length; i++)
                {
                    if (tracks[x].MidiEvents[i].midiMetaEvent == eventMetaType && tracks[x].MidiEvents[i].midiChannelEvent == eventChannelType)
                        matchList.Add(tracks[x].MidiEvents[i]);
                }
            }
            return matchList;
        }
        public List<MidiEvent> getAllMidiEventsofType(byte channel, MidiHelper.MidiChannelEvent eventChannelType, MidiHelper.MidiMetaEvent eventMetaType)
        {
            List<MidiEvent> matchList = new List<MidiEvent>();
            for (int x = 0; x < tracks.Length; x++)
            {
                for (int i = 0; i < tracks[x].MidiEvents.Length; i++)
                {
                    if (tracks[x].MidiEvents[i].midiMetaEvent == eventMetaType 
                        && tracks[x].MidiEvents[i].midiChannelEvent == eventChannelType 
                        && tracks[x].MidiEvents[i].channel == channel)
                        matchList.Add(tracks[x].MidiEvents[i]);
                }
            }
            return matchList;
        }
        public void CombineTracks()
        {
            if (tracks.Length < 2)
                return;
            int total_eventCount = 0;
            UInt64 total_notesPlayed = 0;
            List<byte> programsUsed = new List<byte>();
            List<byte> DrumprogramsUsed = new List<byte>();
            //Loop to get track info
            for (int x = 0; x < tracks.Length; x++)
            {
                total_eventCount = total_eventCount + tracks[x].MidiEvents.Length;
                total_notesPlayed = total_notesPlayed + tracks[x].NotesPlayed;
                for (int x2 = 0; x2 < tracks[x].Programs.Length; x2++)
                {
                    if (programsUsed.Contains(tracks[x].Programs[x2]) == false)
                        programsUsed.Add(tracks[x].Programs[x2]);
                }
                for (int x2 = 0; x2 < tracks[x].DrumPrograms.Length; x2++)
                {
                    if (DrumprogramsUsed.Contains(tracks[x].DrumPrograms[x2]) == false)
                        DrumprogramsUsed.Add(tracks[x].DrumPrograms[x2]);
                }
            }
            //Now process the midi events
            Dictionary<uint, LinkedList<MidiEvent>> OrderedTrack = new Dictionary<uint, LinkedList<MidiEvent>>(total_eventCount);

            for (int x = 0; x < tracks.Length; x++)
            {
                uint CurrentDeltaTime_ = 0;
                MidiEvent[] TrackSeq = new MidiEvent[tracks[x].MidiEvents.Length];
                tracks[x].MidiEvents.CopyTo(TrackSeq, 0);
                for (int x2 = 0; x2 < tracks[x].MidiEvents.Length; x2++)
                {
                    CurrentDeltaTime_ = CurrentDeltaTime_ + TrackSeq[x2].deltaTime;
                    TrackSeq[x2].deltaTime = CurrentDeltaTime_;
                    if (OrderedTrack.ContainsKey(TrackSeq[x2].deltaTime) == true)
                    {
                        LinkedList<MidiEvent> tmplist;
                        tmplist = OrderedTrack[TrackSeq[x2].deltaTime];
                        tmplist.AddLast(TrackSeq[x2]);
                    }
                    else
                    {
                        LinkedList<MidiEvent> tmplist = new LinkedList<MidiEvent>();
                        tmplist.AddLast(TrackSeq[x2]);
                        OrderedTrack.Add(TrackSeq[x2].deltaTime, tmplist);
                    }
                }
            }
            //Sort the Dictionary
            uint[] keys = new uint[OrderedTrack.Keys.Count];
            OrderedTrack.Keys.CopyTo(keys, 0);
            Array.Sort(keys);

            tracks = new MidiTrack[1];
            LinkedList<MidiEvent>[] ArrayofTrkEvts = new LinkedList<MidiEvent>[OrderedTrack.Values.Count];
            for (int x = 0; x < ArrayofTrkEvts.Length; x++)
                ArrayofTrkEvts[x] = OrderedTrack[keys[x]];
            OrderedTrack.Clear();
            keys = null;
            tracks[0] = new MidiTrack();
            tracks[0].Programs = programsUsed.ToArray();
            tracks[0].DrumPrograms = DrumprogramsUsed.ToArray();
            tracks[0].MidiEvents = new MidiEvent[total_eventCount];
            uint PreviousDeltaTime = 0;
            uint cc = 0;
            for (int x = 0; x < ArrayofTrkEvts.Length; x++)
            {
                LinkedListNode<MidiEvent> tmpN = ArrayofTrkEvts[x].First;
                while (tmpN != null)
                {
                    uint old1 = tmpN.Value.deltaTime;
                    tmpN.Value.deltaTime = (tmpN.Value.deltaTime - PreviousDeltaTime);
                    PreviousDeltaTime = old1;
                    tracks[0].MidiEvents[cc] = tmpN.Value;
                    tracks[0].TotalTime = tracks[0].TotalTime + (ulong)tmpN.Value.deltaTime;
                    tmpN = tmpN.Next;
                    cc++;
                }
            }
            tracks[0].NotesPlayed = (uint)total_notesPlayed;
        }
        //--Static Methods
        public static bool isValidMidiFile(string filename)
        {
            Stream stream = File.Open(filename, FileMode.Open);
            byte[] head = new byte[4];
            stream.Read(head, 0, 4);
            stream.Close();
            if (UTF8Encoding.UTF8.GetString(head, 0, head.Length) == "MThd")
                return true;
            else
                return false;
        }
        //--Private Methods
        private void loadStream(Stream stream)
        {
            byte[] tmp = new byte[4];
            stream.Read(tmp, 0, 4);
            if (UTF8Encoding.UTF8.GetString(tmp, 0, tmp.Length) != "MThd")
                throw new Exception("Not a valid midi file!");
            mheader = new MidiHeader();
            //Read header length
            stream.Read(tmp, 0, 4);
            Array.Reverse(tmp); //Reverse the bytes
            int headerLength = BitConverter.ToInt32(tmp, 0);
            //Read midi format
            tmp = new byte[2];
            stream.Read(tmp, 0, 2);
            Array.Reverse(tmp); //Reverse the bytes
            mheader.setMidiFormat(BitConverter.ToInt16(tmp, 0));
            //Read Track Count
            stream.Read(tmp, 0, 2);
            Array.Reverse(tmp); //Reverse the bytes
            int trackCount = BitConverter.ToInt16(tmp, 0);
            tracks = new MidiTrack[trackCount];
            //Read Delta Time
            stream.Read(tmp, 0, 2);
            Array.Reverse(tmp); //Reverse the bytes
            int delta = BitConverter.ToInt16(tmp, 0);
            mheader.DeltaTiming = (delta & 0x7FFF);
            //Time Format
            mheader.TimeFormat = ((delta & 0x8000) > 0) ? MidiHelper.MidiTimeFormat.FamesPerSecond : MidiHelper.MidiTimeFormat.TicksPerBeat;
            //Begin Reading Each Track
            for (int x = 0; x < trackCount; x++)
            {
                List<byte> Programs = new List<byte>();
                List<byte> DrumPrograms = new List<byte>();
                List<MidiEvent> midiEvList = new List<MidiEvent>();
                tracks[x] = new MidiTrack();
                Programs.Add(0); //assume the track uses program at 0 in case no program changes are used
                DrumPrograms.Add(0);
                tmp = new byte[4];      //reset the size again
                stream.Read(tmp, 0, 4);
                if (UTF8Encoding.UTF8.GetString(tmp, 0, tmp.Length) != "MTrk")
                    throw new Exception("Invalid track!");
                stream.Read(tmp, 0, 4);
                Array.Reverse(tmp); //Reverse the bytes
                int TrackLength = BitConverter.ToInt32(tmp, 0);
                //Read The Rest of The Track
                tmp = new byte[TrackLength];
                stream.Read(tmp, 0, TrackLength);
                int index = 0;
                byte prevByte = 0;
                int prevChan = 0;
                while (index < tmp.Length)
                {
                    UInt16 numofbytes = 0;
                    UInt32 ScrmbledDta = BitConverter.ToUInt32(tmp, index);
                    MidiEvent MEv = new MidiEvent();
                    MEv.deltaTime = GetTime(ScrmbledDta, ref numofbytes);
                    index += 4 - (4 - numofbytes);
                    byte statusByte = tmp[index];
                    int CHANNEL = GetChannel(statusByte);
                    if (statusByte < 0x80)
                    {
                        statusByte = prevByte;
                        CHANNEL = prevChan;
                        index--;
                    }
                    if (statusByte != 0xFF)
                        statusByte &= 0xF0;
                    prevByte = statusByte;
                    prevChan = CHANNEL;
                    switch (statusByte)
                    {
                        case 0x80:
                            {
                                MEv.midiChannelEvent = MidiHelper.MidiChannelEvent.Note_Off;
                                ++index;
                                MEv.channel = (byte)CHANNEL;
                                MEv.Parameters[0] = MEv.channel;
                                MEv.parameter1 = tmp[index++];
                                MEv.parameter2 = tmp[index++];
                                MEv.Parameters[1] = MEv.parameter1;
                                MEv.Parameters[2] = MEv.parameter2;
                            }
                            break;
                        case 0x90:
                            {
                                MEv.midiChannelEvent = MidiHelper.MidiChannelEvent.Note_On;
                                ++index;
                                MEv.channel = (byte)CHANNEL;
                                MEv.Parameters[0] = MEv.channel;
                                MEv.parameter1 = tmp[index++];
                                MEv.parameter2 = tmp[index++];
                                MEv.Parameters[1] = MEv.parameter1;
                                MEv.Parameters[2] = MEv.parameter2;
                                if (MEv.parameter2 == 0x00) //Setting velocity to 0 is actually just turning the note off.
                                    MEv.midiChannelEvent = MidiHelper.MidiChannelEvent.Note_Off;
                                tracks[x].NotesPlayed++;
                            }
                            break;
                        case 0xA0:
                            {
                                MEv.midiChannelEvent = MidiHelper.MidiChannelEvent.Note_Aftertouch;
                                MEv.channel = (byte)CHANNEL;
                                MEv.Parameters[0] = MEv.channel;
                                ++index;
                                MEv.parameter1 = tmp[++index];//note number
                                MEv.parameter2 = tmp[++index];//Amount
                            }
                            break;
                        case 0xB0:
                            {
                                MEv.midiChannelEvent = MidiHelper.MidiChannelEvent.Controller;
                                MEv.channel = (byte)CHANNEL;
                                MEv.Parameters[0] = MEv.channel;
                                ++index;
                                MEv.parameter1 = tmp[index++]; //type
                                MEv.parameter2 = tmp[index++]; //value
                                MEv.Parameters[1] = MEv.parameter1;
                                MEv.Parameters[2] = MEv.parameter2;
                            }
                            break;
                        case 0xC0:
                            {
                                MEv.midiChannelEvent = MidiHelper.MidiChannelEvent.Program_Change;
                                MEv.channel = (byte)CHANNEL;
                                MEv.Parameters[0] = MEv.channel;
                                ++index;
                                MEv.parameter1 = tmp[index++];
                                MEv.Parameters[1] = MEv.parameter1;
                                //record which programs are used by the track
                                if (MEv.channel != 9)
                                {
                                    if (Programs.Contains(MEv.parameter1) == false)
                                        Programs.Add(MEv.parameter1);
                                }
                                else
                                {
                                    if (DrumPrograms.Contains(MEv.parameter1) == false)
                                        DrumPrograms.Add(MEv.parameter1);
                                }
                            }
                            break;
                        case 0xD0:
                            {
                                MEv.midiChannelEvent = MidiHelper.MidiChannelEvent.Channel_Aftertouch;
                                MEv.channel = (byte)CHANNEL;
                                MEv.Parameters[0] = MEv.channel;
                                ++index;
                                //Amount
                                MEv.parameter1 = tmp[++index];
                            }
                            break;
                        case 0xE0:
                            {
                                MEv.midiChannelEvent = MidiHelper.MidiChannelEvent.Pitch_Bend;
                                MEv.channel = (byte)CHANNEL;
                                MEv.Parameters[0] = MEv.channel;
                                ++index;
                                MEv.parameter1 = tmp[++index];
                                MEv.parameter2 = tmp[++index];
                                ushort s = (ushort)MEv.parameter1;
                                s <<= 7;
                                s |= (ushort)MEv.parameter2;
                                MEv.Parameters[1] = ((double)s - 8192.0) / 8192.0;
                            }
                            break;
                        case 0xFF:
                            statusByte = tmp[++index];
                            switch (statusByte)
                            {
                                case 0x00:
                                    MEv.midiMetaEvent = MidiHelper.MidiMetaEvent.Sequence_Number; ++index;
                                    break;
                                case 0x01:
                                    MEv.midiMetaEvent = MidiHelper.MidiMetaEvent.Text_Event; ++index;
                                    //Get the length of the string
                                    MEv.parameter1 = tmp[index++];
                                    MEv.Parameters[0] = MEv.parameter1;
                                    //Set the string in the parameter list
                                    MEv.Parameters[1] = UTF8Encoding.UTF8.GetString(tmp, index, ((int)tmp[index - 1])); index += (int)tmp[index - 1];
                                    break;
                                case 0x02:
                                    MEv.midiMetaEvent = MidiHelper.MidiMetaEvent.Copyright_Notice; ++index;
                                    //Get the length of the string
                                    MEv.parameter1 = tmp[index++];
                                    MEv.Parameters[0] = MEv.parameter1;
                                    //Set the string in the parameter list
                                    MEv.Parameters[1] = UTF8Encoding.UTF8.GetString(tmp, index, ((int)tmp[index - 1])); index += (int)tmp[index - 1];
                                    break;
                                case 0x03:
                                    MEv.midiMetaEvent = MidiHelper.MidiMetaEvent.Sequence_Or_Track_Name; ++index;
                                    //Get the length of the string
                                    MEv.parameter1 = tmp[index++];
                                    MEv.Parameters[0] = MEv.parameter1;
                                    //Set the string in the parameter list
                                    MEv.Parameters[1] = UTF8Encoding.UTF8.GetString(tmp, index, ((int)tmp[index - 1])); index += (int)tmp[index - 1];
                                    break;
                                case 0x04:
                                    MEv.midiMetaEvent = MidiHelper.MidiMetaEvent.Instrument_Name; ++index;
                                    //Set the instrument name
                                    MEv.Parameters[0] = UTF8Encoding.UTF8.GetString(tmp, index + 1, (int)tmp[index]);
                                    index += (int)tmp[index] + 1;
                                    break;
                                case 0x05:
                                    MEv.midiMetaEvent = MidiHelper.MidiMetaEvent.Lyric_Text; ++index;
                                    //Set the lyric string
                                    MEv.Parameters[0] = UTF8Encoding.UTF8.GetString(tmp, index + 1, (int)tmp[index]);
                                    index += (int)tmp[index] + 1;
                                    break;
                                case 0x06:
                                    MEv.midiMetaEvent = MidiHelper.MidiMetaEvent.Marker_Text; ++index;
                                    //Set the marker
                                    MEv.Parameters[0] = UTF8Encoding.UTF8.GetString(tmp, index + 1, (int)tmp[index]);
                                    index += (int)tmp[index] + 1;
                                    break;
                                case 0x07:
                                    MEv.midiMetaEvent = MidiHelper.MidiMetaEvent.Cue_Point; ++index;
                                    //Set the cue point
                                    MEv.Parameters[0] = UTF8Encoding.UTF8.GetString(tmp, index + 1, (int)tmp[index]);
                                    index += (int)tmp[index] + 1;
                                    break;
                                case 0x20:
                                    MEv.midiMetaEvent = MidiHelper.MidiMetaEvent.Midi_Channel_Prefix_Assignment; index++;
                                    //Get the length of the data
                                    MEv.parameter1 = tmp[index++];
                                    MEv.Parameters[0] = MEv.parameter1;
                                    //Set the string in the parameter list
                                    MEv.Parameters[1] = tmp[index++];
                                    break;
                                case 0x2F:
                                    MEv.midiMetaEvent = MidiHelper.MidiMetaEvent.End_of_Track;
                                    index += 2;
                                    break;
                                case 0x51:
                                    MEv.midiMetaEvent = MidiHelper.MidiMetaEvent.Tempo; ++index;
                                    //Get the length of the data
                                    MEv.Parameters[4] = tmp[index++];
                                    //Put the data into an array
                                    byte[] mS = new byte[4]; for (int i = 0; i < 3; i++) mS[i + 1] = tmp[i + index]; index += 3;
                                    //Put it into a readable format
                                    byte[] mS2 = new byte[4]; for (int i = 0; i < 4; i++) mS2[3 - i] = mS[i];
                                    //Get the value from the array
                                    UInt32 Val = BitConverter.ToUInt32(mS2, 0);
                                    //Set the value
                                    MEv.Parameters[0] = Val;
                                    break;
                                case 0x54:
                                    MEv.midiMetaEvent = MidiHelper.MidiMetaEvent.Smpte_Offset; ++index;
                                    int v = tmp[index++];
                                    if (v >= 4)
                                        for (int i = 0; i < 4; i++) MEv.Parameters[i] = tmp[index++];
                                    else
                                        for (int i = 0; i < v; i++) MEv.Parameters[i] = tmp[index++];
                                    for (int i = 4; i < v; i++) index++;
                                    break;
                                case 0x58:
                                    MEv.midiMetaEvent = MidiHelper.MidiMetaEvent.Time_Signature; ++index;
                                    int v1 = tmp[index++];
                                    if (v1 >= 4)
                                        for (int i = 0; i < 4; i++) MEv.Parameters[i] = tmp[index++];
                                    else
                                        for (int i = 0; i < v1; i++) MEv.Parameters[i] = tmp[index++];
                                    for (int i = 4; i < v1; i++) index++;
                                    break;
                                case 0x59:
                                    MEv.midiMetaEvent = MidiHelper.MidiMetaEvent.Key_Signature; ++index;
                                    int v2 = tmp[index++];
                                    if (v2 >= 4)
                                        for (int i = 0; i < 4; i++) MEv.Parameters[i] = tmp[index++];
                                    else
                                        for (int i = 0; i < v2; i++) MEv.Parameters[i] = tmp[index++];
                                    for (int i = 4; i < v2; i++) index++;
                                    break;
                                case 0x7F:
                                    //Sequencer specific events
                                    MEv.midiMetaEvent = MidiHelper.MidiMetaEvent.Sequencer_Specific_Event; ++index;    //increment the indexer
                                    //Get the length of the data
                                    MEv.Parameters[4] = tmp[index++];
                                    //Get the byte length
                                    byte[] len = new byte[(byte)MEv.Parameters[4]];
                                    //get the byte info
                                    for (int i = 0; i < len.Length; i++) len[i] = tmp[index++];
                                    MEv.Parameters[0] = len;
                                    break;
                            }
                            break;
                        //System exclusive
                        case 0xF0:
                            while (tmp[index] != 0xF7)
                                index++;
                            index++;
                            break;
                    }
                    midiEvList.Add(MEv);
                    tracks[x].TotalTime = tracks[x].TotalTime + MEv.deltaTime;
                }
                tracks[x].Programs = Programs.ToArray();
                tracks[x].DrumPrograms = DrumPrograms.ToArray();
                tracks[x].MidiEvents = midiEvList.ToArray();
            }
        }
        private int GetChannel(byte statusbyte)
        {
            statusbyte = (byte)(statusbyte << 4);
            return statusbyte >> 4;
        }
        private uint GetTime(UInt32 data, ref UInt16 numOfBytes)
        {
            byte[] buff = BitConverter.GetBytes(data); numOfBytes++;
            for (int i = 0; i < buff.Length; i++) { if ((buff[i] & 0x80) > 0) { numOfBytes++; } else { break; } }
            for (int i = numOfBytes; i < 4; i++) buff[i] = 0x00;
            Array.Reverse(buff);
            data = BitConverter.ToUInt32(buff, 0);
            data >>= (32 - (numOfBytes * 8));
            UInt32 b = data;
            UInt32 bffr = (data & 0x7F);
            int c = 1;
            while ((data >>= 8) > 0)
            {
                bffr |= ((data & 0x7F) << (7 * c)); c++;
            }
            return bffr;
        }
    }
}
