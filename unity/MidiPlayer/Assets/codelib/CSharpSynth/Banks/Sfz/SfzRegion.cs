namespace CSharpSynth.Banks.Sfz
{
    public class SfzRegion
    {
        //--Variables
        public int SampleIndex; //0 to max.Int
        public int Release;   //Samples
        public int Attack;    //Samples 
        public int Hold;      //Samples
        public int Decay;     //Samples
        public float Tune;      //-1 to 1 Semitones
        public int Root;        //-127 to 127
        public byte HiNote;     //0 to 127
        public byte LoNote;     //0 to 127
        public byte HiVelocity; //0 to 127
        public byte LoVelocity; //0 to 127
        public byte HiChannel;  //0 to 15
        public byte LoChannel;  //0 to 15
        public int LoopStart;   //0 to max.Int
        public int LoopEnd;     //0 to max.Int
        public float Volume;    //?
        public byte LoopMode;   //0 no loop, 1 continuous loop, 2 sustain loop
        public float Pan;       //-1.0 to 1.0
        public int Offset;      //0 to max.Int
        public float Effect1;   //0.0% to 100% reverb
        public float Effect2;   //0.0% to 100% chorus
        //--Public Methods
        public SfzRegion()
        {
            //assign default values
            SampleIndex = 0;
            Release = 0;
            Attack = 0;
            Hold = 0;
            Decay = int.MaxValue;
            Tune = 0.0f;
            Root = 60;
            HiNote = 127;
            LoNote = 0;
            HiVelocity = 127;
            LoVelocity = 0;
            HiChannel = 15;
            LoChannel = 0;
            LoopStart = 0;
            LoopEnd = 0;
            Volume = 0.0f;
            LoopMode = 0;
            Pan = 0.0f;
            Offset = 0;
            Effect1 = 0.0f;
            Effect2 = 0.0f;
        }
        public bool isWithinRegion(int channel, int note, int velocity)
        {
            if (channel >= this.LoChannel && channel <= this.HiChannel &&
                note >= this.LoNote && note <= this.HiNote &&
                velocity >= this.LoVelocity && velocity <= this.HiVelocity)
                return true;
            return false;
        }
        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(SfzRegion))
            {
                SfzRegion r = (SfzRegion)obj;
                if (r.HiNote == this.HiNote && r.LoNote == this.LoNote && r.LoopEnd == this.LoopEnd &&
                    r.LoopMode == this.LoopMode && r.LoopStart == this.LoopStart &&
                    r.Release == this.Release && r.Root == this.Root && r.SampleIndex == this.SampleIndex &&
                    r.Tune == this.Tune && r.Volume == this.Volume && r.Decay == this.Decay &&
                    r.HiChannel == this.HiChannel && r.LoChannel == this.LoChannel &&
                    r.HiVelocity == this.HiVelocity && r.LoVelocity == this.LoVelocity &&
                    r.Pan == this.Pan && r.Offset == this.Offset && r.Effect1 == this.Effect1 &&
                    r.Effect2 == this.Effect2)
                    return true;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
