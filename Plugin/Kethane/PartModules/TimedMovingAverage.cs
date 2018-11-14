using System;
using System.Collections.Generic;

namespace Kethane.PartModules
{
    internal class TimedMovingAverage
    {
        private struct TimedValue
        {
            public readonly double Time;
            public readonly double Value;
            public TimedValue(double time, double value)
            {
                Time = time;
                Value = value;
            }
        }

        private readonly Queue<TimedValue> values = new Queue<TimedValue>();
        private readonly double interval;

        public TimedMovingAverage(double interval, double initialValue = 0)
        {
            this.interval = interval;
            values.Enqueue(new TimedValue(interval, initialValue));
        }

        public void Update(double time, double value)
        {
            values.Enqueue(new TimedValue(time, value));
        }

        public double Average
        {
            get
            {
                double time = 0;
                double value = 0;
                int removing = values.Count;

                foreach (var entry in values)
                {
                    removing--;
                    if (time + entry.Time > interval)
                    {
                        value += entry.Value * (interval - time);
                        break;
                    }
                    else
                    {
                        time += entry.Time;
                        value += entry.Value * entry.Time;
                    }
                }

                while (removing > 0)
                {
                    removing--;
                    values.Dequeue();
                }

                return value / interval;
            }
        }
    }
}
