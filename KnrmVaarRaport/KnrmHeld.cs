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
        internal SortedDictionary<string, BaseData> SdBoot { get; private set; }
        internal KnrmHeld()
        {
            SdInzet = new SortedDictionary<string, TypeInzet>();
            SdBoot = new SortedDictionary<string, BaseData>();
        }

        internal void AddInzet(TypeInzet typeInzet, double hours)
        {
            if (!SdInzet.ContainsKey(typeInzet.Name))
                SdInzet.Add(typeInzet.Name, new TypeInzet() { Name = typeInzet.Name });
            SdInzet.TryGetValue(typeInzet.Name, out var inzetObject);
            inzetObject.Count++;
            inzetObject.SetHours(hours);
        }

        internal void AddBoot(BaseData boot, double hours)
        {
            if (!SdBoot.ContainsKey(boot.Name))
                SdBoot.Add(boot.Name, new BaseData() { Name = boot.Name });
            SdBoot.TryGetValue(boot.Name, out var bootObject);
            bootObject.Count++;
            bootObject.SetHours(hours);
        }
    }
}
