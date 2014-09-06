namespace CSharpSynth.Midi
{
    public static class MidiHelper
    {
        //--Constants
        public const uint MicroSecondsPerMinute = 60000000; //microseconds in a minute
        public const int Max_MidiChannel = 15;
        public const int Min_MidiChannel = 0;
        public const int Drum_Channel = 9;
        public const byte Max_NoteNumber = 127;
        public const byte Min_NoteNumber = 0;
        public const byte Max_Velocity = 127;
        public const byte Min_Velocity = 0;
        public const byte Max_Controller = 127;
        public const byte Min_Controller = 0;
        public const byte Max_GenericParameter = 127;
        public const byte Min_GenericParameter = 0;
        //--Enum
        public enum MidiTimeFormat
        {
            TicksPerBeat,
            FamesPerSecond
        }
        public enum MidiChannelEvent
        {
            None,
            Note_On,
            Note_Off,
            Note_Aftertouch,
            Controller,
            Program_Change,
            Channel_Aftertouch,
            Pitch_Bend,
            Unknown
        }
        public enum ControllerType
        {
            None,
            BankSelect,
            Modulation,
            BreathController,
            FootController,
            PortamentoTime,
            DataEntry,
            MainVolume,
            Balance,
            Pan,
            ExpressionController,
            EffectControl1,
            EffectControl2,
            GeneralPurposeController1,
            GeneralPurposeController2,
            GeneralPurposeController3,
            GeneralPurposeController4,
            DamperPedal,
            Portamento,
            Sostenuto,
            SoftPedal,
            LegatoFootswitch,
            Hold2,
            SoundController1,
            SoundController2,
            SoundController3,
            SoundController4,
            SoundController6,
            SoundController7,
            SoundController8,
            SoundController9,
            SoundController10,
            GeneralPurposeController5,
            GeneralPurposeController6,
            GeneralPurposeController7,
            GeneralPurposeController8,
            PortamentoControl,
            Effects1Depth,
            Effects2Depth,
            Effects3Depth,
            Effects4Depth,
            Effects5Depth,
            DataIncrement,
            DataDecrement,
            NonRegisteredParameter,
            RegisteredParameter,
            ResetControllers,
            AllNotesOff,
            OmniModeOn,
            OmniModeOff,
            Unknown
        }
        public enum MidiMetaEvent
        {
            None,
            Sequence_Number,
            Text_Event,
            Copyright_Notice,
            Sequence_Or_Track_Name,
            Instrument_Name,
            Lyric_Text,
            Marker_Text,
            Cue_Point,
            Midi_Channel_Prefix_Assignment,
            End_of_Track,
            Tempo,
            Smpte_Offset,
            Time_Signature,
            Key_Signature,
            Sequencer_Specific_Event,
            Unknown
        }
        public enum MidiFormat
        {
            SingleTrack,
            MultiTrack,
            MultiSong
        }
    }
}
