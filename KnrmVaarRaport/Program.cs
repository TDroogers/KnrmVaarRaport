using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace KnrmVaarRaport
{
    public static class Program
    {
        private static string _path = string.Empty;
        private static readonly SortedDictionary<string, KnrmHeld> _sdHelden = new();
        private static readonly SortedDictionary<string, TypeInzet> _sdInzet = new();
        private static readonly SortedDictionary<string, BaseData> _sdBoot = new();
        private static readonly List<string> _typeInzetToIgnore = new() { "Boot uitgemeld" };
        public static void Main(string[] args)
        {
            Work();
        }

        private static void Work()
        {
            try
            {
                ReadFileActierapporten();
                ReadFileOverig();
                //AskToContinue();
                var timeTicks = DateTime.Now.Ticks;
                WriteResultFile(timeTicks);
                WriteInzetToFile(timeTicks);
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }

        private static void ReadFileActierapporten()
        {
            using var sr = new StreamReader("Actierapporten.csv");
            int i = 0;
            int fieldCount = 0;
            string[] Titles;
            while (true)
            {
                var line = sr.ReadLine();
                if (line == null) break;
                var inzet = SplitCsv.Split(line, sr);
                if (i != 0)
                {
                    if (fieldCount != inzet.Count())
                    {
#if DEBUG
                        Debugger.Break();
#endif
                        continue;
                    }
                    var omschrijving = inzet[3];
                    if (_typeInzetToIgnore.Contains(omschrijving))
                        continue;
                    var hours = CalculateHours(DateTime.Parse(inzet[9]), DateTime.Parse(inzet[11]));
                    var schipper = inzet[2];
                    var opstapper1 = inzet[39];
                    var opstapper2 = inzet[40];
                    var opstapper3 = inzet[41];
                    var opstapper4 = inzet[42];
                    var opstapper5 = inzet[43];
                    var datum = inzet[0];
                    var weer = inzet[38];
                    var windkracht = inzet[16];
                    var windrichting = inzet[15];
                    var zicht = inzet[17];
                    var oproepGedaanDoor = inzet[7];
                    var andereHulpverleners = SplitCsv.ToArray(inzet[12]);
                    var vaartuiggroep = inzet[33];
                    var oorzaken = inzet[25];
                    var positie = inzet[18];
                    var prio = inzet[34];
                    int.TryParse(inzet[21], out int aantalGeredden);
                    int.TryParse(inzet[22], out int aantalDieren);
                    int.TryParse(inzet[20], out int aantalOpvarende);
                    var behoevenVan = inzet[33];
                    var boot = UpdateBoot(inzet[6], hours);
                    var typeInzet = UpdateTypeInzet(inzet[3], hours, boot, weer, windkracht, andereHulpverleners, aantalGeredden, aantalDieren, aantalOpvarende, behoevenVan, vaartuiggroep, oorzaken, positie, prio, windrichting, zicht, oproepGedaanDoor);
                    UpdateHelpers(hours, schipper, opstapper1, opstapper2, opstapper3, opstapper4, opstapper5, boot, typeInzet);
                }
                else
                {
                    fieldCount = inzet.Count();
                    Titles = inzet;
                }
                i++;
            }
        }

        private static void ReadFileOverig()
        {
            using var sr = new StreamReader("Overige rapporten.csv");
            int i = 0;
            int fieldCount = 0;
            string[] Titles;
            while (true)
            {
                var line = sr.ReadLine();
                if (line == null) break;
                var inzet = SplitCsv.Split(line, sr);
                if (i != 0)
                {
                    if (fieldCount != inzet.Count())
                    {
#if DEBUG
                        Debugger.Break();
#endif
                        continue;
                    }
                    var omschrijving = inzet[1];
                    if (_typeInzetToIgnore.Contains(omschrijving))
                        continue;
                    var hours = CalculateHours(DateTime.Parse(inzet[4]), DateTime.Parse(inzet[5]));
                    var schipper = inzet[21];
                    var opstapper1 = inzet[22];
                    var opstapper2 = inzet[23];
                    var opstapper3 = inzet[24];
                    var opstapper4 = inzet[25];
                    var opstapper5 = inzet[26];
                    var datum = inzet[0];
                    var weer = inzet[9];
                    var windkracht = inzet[8];
                    var windrichting = inzet[7];
                    var zicht = inzet[10];
                    var andereHulpverleners = SplitCsv.ToArray(inzet[6]);
                    var boot = UpdateBoot(inzet[3], hours);
                    var typeInzet = UpdateTypeInzet(inzet[1], hours, boot, weer, windkracht, andereHulpverleners, 0, 0, 0, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, windrichting, zicht, string.Empty);

                    UpdateHelpers(hours, schipper, opstapper1, opstapper2, opstapper3, opstapper4, opstapper5, boot, typeInzet);
                }
                else
                {
                    fieldCount = inzet.Count();
                    Titles = inzet;
                }
                i++;
            }
        }

        private static void UpdateHelpers(double hours, string schipper, string opstapper1, string opstapper2, string opstapper3, string opstapper4, string opstapper5, BaseData boot, TypeInzet typeInzet)
        {
            UpdateKnrmHelper(schipper, hours, typeInzet, boot);
            if (string.Compare(schipper, opstapper1) != 0)
                UpdateKnrmHelper(opstapper1, hours, typeInzet, boot);
            if (!(schipper + opstapper1).Contains(opstapper2, StringComparison.CurrentCulture))
                UpdateKnrmHelper(opstapper2, hours, typeInzet, boot);
            if (!(schipper + opstapper1 + opstapper2).Contains(opstapper3, StringComparison.CurrentCulture))
                UpdateKnrmHelper(opstapper3, hours, typeInzet, boot);
            if (!(schipper + opstapper1 + opstapper2 + opstapper3).Contains(opstapper4, StringComparison.CurrentCulture))
                UpdateKnrmHelper(opstapper4, hours, typeInzet, boot);
            if (!(schipper + opstapper1 + opstapper2 + opstapper3 + opstapper4).Contains(opstapper5, StringComparison.CurrentCulture))
                UpdateKnrmHelper(opstapper5, hours, typeInzet, boot);
        }

        private static double CalculateHours(DateTime oproep, DateTime terug)
        {
            var vaarTijd = terug.Subtract(oproep);
            double hours = vaarTijd.TotalHours + (vaarTijd.Minutes / 60);
            return hours;
        }

        private static TypeInzet UpdateTypeInzet(string typeInzet, double hours, BaseData boot, string weer, string windkracht, string[] andereHulpverleners, int aantalGeredden, int aantalDieren, int aantalOpvarende, string behoevenVan, string vaartuiggroep, string oorzaken, string positie, string prio, string windrichting, string zicht, string oproepGedaanDoor)
        {
            if (!_sdInzet.ContainsKey(typeInzet))
                _sdInzet.Add(typeInzet, new TypeInzet() { Name = typeInzet });
            if (behoevenVan is null)
            {
                throw new ArgumentNullException(nameof(behoevenVan));
            }

            _sdInzet.TryGetValue(typeInzet, out var inzetObject);
            inzetObject.Count++;
            inzetObject.SetHours(hours);
            inzetObject.AddData(hours, boot, weer, windkracht, andereHulpverleners, aantalGeredden, aantalDieren, aantalOpvarende, behoevenVan, vaartuiggroep, oorzaken, positie, prio, windrichting, zicht, oproepGedaanDoor);
            return inzetObject;
        }

        private static BaseData UpdateBoot(string boot, double hours)
        {
            if (!_sdBoot.ContainsKey(boot))
                _sdBoot.Add(boot, new BaseData() { Name = boot });
            _sdBoot.TryGetValue(boot, out var bootObject);
            bootObject.Count++;
            bootObject.SetHours(hours);
            return bootObject;
        }

        private static void UpdateKnrmHelper(string redder, double hours, TypeInzet typeInzet, BaseData boot)
        {
            if (string.IsNullOrWhiteSpace(redder) || string.Compare(redder, "n.v.t.", true) == 0 || string.Compare(redder, "nee", true) == 0 || int.TryParse(redder, out int ignore))
                return;
            if (!_sdHelden.ContainsKey(redder))
                _sdHelden.Add(redder, new KnrmHeld() { Name = redder });
            _sdHelden.TryGetValue(redder, out var held);
            if (held == null)
                return;
            held.AddInzet(typeInzet, hours);
            held.AddBoot(boot, hours);
            held.Count++;
            held.SetHours(hours);
        }

        private static void AskToContinue()
        {
            Console.Write("Druk op enter om het resultaat te verwerken ;)");
            while (Console.ReadKey().Key != ConsoleKey.Enter) { }
        }

        private static void WriteResultFile(long timeTicks)
        {
            using FileStream fs = File.Create("result-" + timeTicks + ".csv");
            byte[] row = GetRowTitle();
            fs.Write(row, 0, row.Length);
            foreach (var held in _sdHelden.Values)
            {
                row = GetRowHeld(held);
                fs.Write(row, 0, row.Length);
            }
        }

        private static byte[] GetRowTitle()
        {
            var rowTitle = "Redder;totaal inzet;totaal uren;totaal hele uren;";
            foreach (var inzet in _sdInzet.Values)
            {
                rowTitle += string.Format("{0} aantal;{0} uren; {0} hele uren;", inzet.Name);
            }
            foreach (var inzet in _sdBoot.Values)
            {
                rowTitle += string.Format("{0} aantal;{0} uren; {0} hele uren;", inzet.Name);
            }
            return new UTF8Encoding(true).GetBytes(string.Format("{0}\r\n", rowTitle));
        }

        private static byte[] GetRowHeld(KnrmHeld held)
        {
            var rowHeld = string.Format("{0};{1};{2};{3};", held.Name, held.Count, held.Hours.ToString("F1", CultureInfo.InvariantCulture), held.HoursUp);
            foreach (var inzet in _sdInzet.Values)
            {
                if (held.SdInzet.TryGetValue(inzet.Name, out var inzetHeld))
                    rowHeld += string.Format("{0};{1};{2};", inzetHeld.Count, inzetHeld.Hours.ToString("F1", CultureInfo.InvariantCulture), inzetHeld.HoursUp);
                else
                    rowHeld += "0;0;0;";
            }
            foreach (var inzet in _sdBoot.Values)
            {
                if (held.SdBoot.TryGetValue(inzet.Name, out var bootHeld))
                    rowHeld += string.Format("{0};{1};{2};", bootHeld.Count, bootHeld.Hours.ToString("F1", CultureInfo.InvariantCulture), bootHeld.HoursUp);
                else
                    rowHeld += "0;0;0;";
            }
            return new UTF8Encoding(true).GetBytes(string.Format("{0}\r\n", rowHeld));
        }

        private static void WriteInzetToFile(long timeTicks)
        {
            foreach (var typeInzet in _sdInzet)
            {
                var name = typeInzet.Value.Name;
                foreach (var c in Path.GetInvalidFileNameChars()) { name = name.Replace(c, '-'); }
                using FileStream fs = File.Create($"{name}-{timeTicks}.csv");
                byte[] row = new UTF8Encoding(true).GetBytes(string.Format("{0}\r\n", name));
                fs.Write(row, 0, row.Length);
                row = new UTF8Encoding(true).GetBytes(string.Format("uren;{0};Geredde dieren;{1};Geredde personen;{2};Opvarende;{3};\r\n", typeInzet.Value.Hours, typeInzet.Value.AantalDieren, typeInzet.Value.AantalGeredden, typeInzet.Value.AantalOpvarende));
                fs.Write(row, 0, row.Length);
                row = AddTypesToDocument(typeInzet.Value.SdBoot, fs, "boot");
                row = AddTypesToDocument(typeInzet.Value.SdWeer, fs, "weer");
                row = AddTypesToDocument(typeInzet.Value.SdWindkracht, fs, "windkracht");
                row = AddTypesToDocument(typeInzet.Value.SdWindrichting, fs, "Windrichting");
                row = AddTypesToDocument(typeInzet.Value.SdAndereHulpverleners, fs, "Andere hulpverleners");
                row = AddTypesToDocument(typeInzet.Value.SdBehoevenVan, fs, "behoeven van");
                row = AddTypesToDocument(typeInzet.Value.SdVaartuiggroep, fs, "vaartuiggroep");
                row = AddTypesToDocument(typeInzet.Value.SdPositie, fs, "Positie");
                row = AddTypesToDocument(typeInzet.Value.SdOorzaak, fs, "Oorzaak");
                row = AddTypesToDocument(typeInzet.Value.SdPrio, fs, "Prio");
                row = AddTypesToDocument(typeInzet.Value.SdZicht, fs, "Zicht");
                row = AddTypesToDocument(typeInzet.Value.SdOproepGedaanDoor, fs, "Oproep gedaan door");
            }
        }

        private static byte[] AddTypesToDocument(SortedDictionary<string, BaseData> type, FileStream fs, string omschrijving)
        {
            byte[] row = new UTF8Encoding(true).GetBytes(string.Format("{0};{1};\r\n", omschrijving, type.Count));
            fs.Write(row, 0, row.Length);
            foreach (var weer in type)
            {
                row = new UTF8Encoding(true).GetBytes(string.Format("Naam;{0};aantal;{1};uren;{2};\r\n", weer.Value.Name, weer.Value.Count, weer.Value.Hours));
                fs.Write(row, 0, row.Length);
            }
            row = new UTF8Encoding(true).GetBytes(string.Format("\r\n"));
            fs.Write(row, 0, row.Length);
            return row;
        }
    }
}