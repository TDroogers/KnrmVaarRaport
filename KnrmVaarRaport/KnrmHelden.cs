using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnrmVaarRaport
{
    internal class KnrmHelden
    {
        internal string Name { get; private set; }
        internal int Actions { get; private set; }
        internal int Training { get; private set; }
        internal int TotalActivities { get { return Actions + Training; } }
        internal double HoursActions { get; private set; }
        internal double HoursTraining { get; private set; }
        internal double HoursTotal { get { return HoursActions + HoursTraining; } }
        internal KnrmHelden(string name)
        {
            Name = name;
        }
        internal void AddAction()
        {
            Actions++;
        }

        internal void AddTraining()
        {
            Training++;
        }

        internal void AddActionsHours(double hours)
        {
            HoursActions += hours;
        }

        internal void AddTrainingHours(double hours)
        {
            HoursTraining += hours;
        }
    }
}
