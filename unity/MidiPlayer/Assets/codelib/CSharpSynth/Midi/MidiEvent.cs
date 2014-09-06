namespace CSharpSynth.Midi
{
    public class MidiEvent
    {
        //--Variables
        public uint deltaTime;
        public MidiHelper.MidiMetaEvent midiMetaEvent;
        public MidiHelper.MidiChannelEvent midiChannelEvent;
        public object[] Parameters;
        public byte parameter1;
        public byte parameter2;
        public byte channel;
        //--Public Methods
        public MidiEvent()
        {
            this.Parameters = new object[5];
            this.midiMetaEvent = MidiHelper.MidiMetaEvent.None;
            this.midiChannelEvent = MidiHelper.MidiChannelEvent.None;
        }
        public bool isMetaEvent()
        {
            return midiChannelEvent == MidiHelper.MidiChannelEvent.None;
        }
        public bool isChannelEvent()
        {
            return midiMetaEvent == MidiHelper.MidiMetaEvent.None;
        }
        public MidiHelper.ControllerType GetControllerType()
        {
            if (midiChannelEvent != MidiHelper.MidiChannelEvent.Controller)
                return MidiHelper.ControllerType.None;
            switch (parameter1)
            {
                case 1:
                    return MidiHelper.ControllerType.Modulation;
                case 7:
                    return MidiHelper.ControllerType.MainVolume;
                case 10:
                    return MidiHelper.ControllerType.Pan;
                case 64:
                    return MidiHelper.ControllerType.DamperPedal;
                case 121:
                    return MidiHelper.ControllerType.ResetControllers;
                case 123:
                    return MidiHelper.ControllerType.AllNotesOff;
                default:
                    return MidiHelper.ControllerType.Unknown;
            }
        }
    }
}
