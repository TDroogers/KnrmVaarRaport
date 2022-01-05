using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnrmVaarRaport
{
    internal class KnrmHeld : BaseData
    {
        internal SortedDictionary<string, TypeInzet> SdInzet { get; private set; }
        internal SortedDictionary<string, Boot> SdBoot { get; private set; }
        internal KnrmHeld()
        {
            SdInzet = new SortedDictionary<string, TypeInzet>();
            SdBoot = new SortedDictionary<string, Boot>();
        }

        internal void AddInzet(TypeInzet typeInzet, double hours)
        {
            if (!SdInzet.ContainsKey(typeInzet.Name))
                SdInzet.Add(typeInzet.Name, new TypeInzet() { Name = typeInzet.Name });
            SdInzet.TryGetValue(typeInzet.Name, out var inzetObject);
            inzetObject.Count++;
            inzetObject.SetHours(hours);
        }

        internal void AddBoot(Boot boot, double hours)
        {
            if (!SdBoot.ContainsKey(boot.Name))
                SdBoot.Add(boot.Name, new Boot() { Name = boot.Name });
            SdBoot.TryGetValue(boot.Name, out var bootObject);
            bootObject.Count++;
            bootObject.SetHours(hours);
        }
    }
}
