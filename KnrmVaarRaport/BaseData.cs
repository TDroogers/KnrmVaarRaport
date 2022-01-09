using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnrmVaarRaport
{
    internal class BaseData
    {
        internal string Name { get; set; }
        internal int Count { get; set; }
        internal double Hours { get; private set; }
        internal int HoursUp { get; private set; }

        internal void SetHours(double hours)
        {
            Hours += hours;
            HoursUp += (int)Math.Ceiling(hours);
        }
    }
}
