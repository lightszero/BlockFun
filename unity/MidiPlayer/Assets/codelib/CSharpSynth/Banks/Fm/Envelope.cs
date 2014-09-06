using System;
using CSharpSynth.Synthesis;

namespace CSharpSynth.Banks.Fm
{
    public class Envelope
    {
        //--Variables
        private double[] timePoints;
        private double[] valuPoints;
        private int arraylength = 0;
        private double maxTime = 0;
        private bool loop = true;
        private double peak = 1;
        //--Public Properties
        public bool Looping
        {
            get { return loop; }
            set { loop = value; }
        }
        public double Peak
        {
            get { return peak; }
            set { peak = value; }
        }
        //--Public Methods
        public Envelope(double[] timePoints, double[] valuPoints)
        {
            if (timePoints.Length != valuPoints.Length)
                throw new IndexOutOfRangeException("Envelope params must have matching lengths.");
            this.timePoints = timePoints;
            clampArray(valuPoints);
            this.valuPoints = valuPoints;
            arraylength = timePoints.Length;
            sort();
            recalculateMaxTime();
        }
        public Envelope(Func<double, double> function, double time, double start, double end, int size)
        {
            double[] timePoints = new double[size + 1];
            double[] valuPoints = new double[size + 1];
            decimal delta = (decimal)(time / size);
            decimal start_ = (decimal)start;
            decimal end_ = (decimal)end;
            decimal inc = (end_ - start_) / size;
            decimal x;
            int indexcounter = 0;
            if (start_ < end_)
            {
                for (x = start_; x <= end_; x += inc)
                {
                    timePoints[indexcounter] = (double)(indexcounter * delta);
                    valuPoints[indexcounter] = function.Invoke((double)x);
                    indexcounter++;
                }
            }
            else
            {
                for (x = start_; x >= end_; x += inc)
                {
                    timePoints[indexcounter] = (double)(indexcounter * delta);
                    valuPoints[indexcounter] = function.Invoke((double)x);
                    indexcounter++;
                }
            }
            double maxvalue = 0;
            double minvalue = 0;
            //Move function up if parts are negative until its all in the positive.
            for (int x2 = 0; x2 < size + 1; x2++)
            {
                if (valuPoints[x2] < minvalue)
                    minvalue = valuPoints[x2];
            }
            //Get the biggest element.
            for (int x2 = 0; x2 < size + 1; x2++)
            {
                valuPoints[x2] = valuPoints[x2] + (minvalue * -1);
                if (valuPoints[x2] > maxvalue)
                    maxvalue = valuPoints[x2];
            }
            //Now scale the values to the time.
            if (maxvalue != 0)
            {
                for (int x2 = 0; x2 < size + 1; x2++)
                {
                    valuPoints[x2] = Math.Abs((valuPoints[x2] / maxvalue) * time);
                }
            }
            this.timePoints = timePoints;
            clampArray(valuPoints);
            this.valuPoints = valuPoints;
            arraylength = timePoints.Length;
            sort();
            recalculateMaxTime();
        }
        public void addPoint(double time, double value)
        {
            value = SynthHelper.Clamp(value, 0.0, 1.0);
            if (contains(timePoints, time))
            {
                replace(time, value);
            }
            else
            {
                if (arraylength == timePoints.Length)
                    resize();
                timePoints[arraylength] = time;
                valuPoints[arraylength] = value;
                arraylength++;
                if (time > maxTime)
                    maxTime = time;
            }
            sort();
        }
        public float doProcess(double time)
        {
            if (loop == false)
            {
                if (time >= maxTime)
                    time = maxTime;
            }
            else
            {
                if (time >= maxTime)
                    time = time % maxTime;
            }
            for (int x = 0; x < arraylength; x++)
            {
                if (timePoints[x] > time)
                {
                    double slope = (valuPoints[x - 1] - valuPoints[x]) / (timePoints[x - 1] - timePoints[x]);
                    double b = valuPoints[x] - (slope * timePoints[x]);
                    return (float)(((slope * time) + b) * peak);
                }
            }
            return 0;
        }
        //--Private Methods
        private bool contains(double[] array, double value)
        {
            for (int x = 0; x < arraylength; x++)
            {
                if (array[x] == value)
                    return true;
            }
            return false;
        }
        private bool contains(float[] array, float value)
        {
            for (int x = 0; x < arraylength; x++)
            {
                if (array[x] == value)
                    return true;
            }
            return false;
        }
        private bool replace(double time, double newValue)
        {
            for (int x = 0; x < arraylength; x++)
            {
                if (timePoints[x] == time)
                {
                    valuPoints[x] = newValue;
                    return true;
                }
            }
            return false;
        }
        private void recalculateMaxTime()
        {
            maxTime = 0.0;
            for (int x = 0; x < arraylength; x++)
            {
                if (timePoints[x] > maxTime)
                    maxTime = timePoints[x];
            }
        }
        private void resize()
        {
            const int growth = 5;
            double[] timePoints2 = new double[timePoints.Length + growth];
            double[] valuPoints2 = new double[valuPoints.Length + growth];
            for (int x = 0; x < arraylength; x++)
            {
                timePoints2[x] = timePoints[x];
                valuPoints2[x] = valuPoints[x];
            }
            valuPoints = valuPoints2;
            timePoints = timePoints2;
        }
        private void sort()
        {
            double[] timePoints2 = new double[timePoints.Length];
            double[] valuPoints2 = new double[valuPoints.Length];

            double tmp = -1.0;
            int counter = 0;

            for (int y = 0; y < arraylength; y++)
            {
                for (int x = 0; x < arraylength; x++)
                {
                    if (tmp < timePoints[x])
                    {
                        timePoints2[counter] = timePoints[x];
                        valuPoints2[counter] = valuPoints[x];
                        tmp = timePoints[x];
                        counter++;
                        break;
                    }
                }
            }
            timePoints = timePoints2;
            valuPoints = valuPoints2;
        }
        private void clampArray(double[] array)
        {
            for (int x = 0; x < array.Length; x++)
            {
                array[x] = SynthHelper.Clamp(array[x], 0.0, 1.0);
            }
        }
        //--Static
        public static Envelope CreateBasicFadeIn(double maxTime)
        {
            return new Envelope(new double[] { 0.0, maxTime }, new double[] { 0.0, 1.0 });
        }
        public static Envelope CreateBasicFadeOut(double maxTime)
        {
            return new Envelope(new double[] { 0.0, maxTime }, new double[] { 1.0, 0.0 });
        }
        public static Envelope CreateBasicFadeInAndOut(double fadeInTime, double fadeOutTime)
        {
            return new Envelope(new double[] { 0.0, fadeInTime, fadeInTime + fadeOutTime }, new double[] { 0.0, 1.0, 0.0 });
        }
        public static Envelope CreateBasicConstant()
        {
            return new Envelope(new double[] { 0.0, 1.0 }, new double[] { 1.0, 1.0 });
        }
    }
}
