using System.Collections.Generic;
using CSharpSynth.Midi;

namespace CSharpSynth.Sequencer
{
    public class MidiSequencerEvent
    {
        //--Variables
        public List<MidiEvent> Events; //List of Events
        //--Public Methods
        public MidiSequencerEvent()
        {
            Events = new List<MidiEvent>();
        }
    }
}
