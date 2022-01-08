using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace KnrmVaarRaport
{
    public class Program
    {
        private static string _path = string.Empty;
        private static SortedDictionary<string, KnrmHeld> _sdHelden;
        private static SortedDictionary<string, TypeInzet> _sdInzet;
        private static SortedDictionary<string, Boot> _sdBoot;
        public static void Main(string[] args)
        {
            if (ReadArgs(args) && !string.IsNullOrEmpty(_path))
            {
                Work();
            }
        }

        private static bool ReadArgs(string[] args)
        {
            if (args.Length == 0 || args[0] == null)
                return false;
            _path = args[0];
            return true;
        }

        private static void Work()
        {
            try
            {
                if (_path == null) return;
                _sdHelden = new SortedDictionary<string, KnrmHeld>();
                _sdInzet = new SortedDictionary<string, TypeInzet>();
                _sdBoot = new SortedDictionary<string, Boot>();
                ReadFile();
                AskToContinue();
                WriteResultFile();
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }

        private static void ReadFile()
        {
            using var sr = new StreamReader(_path);//"VAAR2021.csv"
            int i = 0;
            int fieldCount = 0;
            while (true)
            {
                var line = sr.ReadLine();
                if (line == null) break;
                var inzet = SplitCsv.Split(line);
                if (i != 0)
                {
                    if (fieldCount != inzet.Count())
                    {
#if DEBUG
                        Debugger.Break();
#endif
                        continue;
                    }
                    var omschrijving = inzet[11];
                    var isInzet = Regex.IsMatch(omschrijving, "^[0-9].*-");
                    var hours = double.Parse(inzet[10].Replace(',', '.'));
                    var schipper = inzet[15];//10 voor kort
                    var opstapper1 = inzet[16];
                    var opstapper2 = inzet[17];
                    var opstapper3 = inzet[18];
                    var opstapper4 = inzet[19];
                    var opstapper5 = inzet[20];
                    var datum = inzet[1];
                    var typeInzet = UpdateTypeInzet(inzet[2], hours);
                    var boot = UpdasteBoot(inzet[3], hours);

                    Console.WriteLine($"{isInzet} {typeInzet} {omschrijving}");
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
                else
                    fieldCount = inzet.Count();
                i++;
            }
        }

        private static TypeInzet UpdateTypeInzet(string typeInzet, double hours)
        {
            if (!_sdInzet.ContainsKey(typeInzet))
                _sdInzet.Add(typeInzet, new TypeInzet() { Name = typeInzet });
            _sdInzet.TryGetValue(typeInzet, out var inzetObject);
            inzetObject.Count++;
            inzetObject.SetHours(hours);
            return inzetObject;
        }

        private static Boot UpdasteBoot(string boot, double hours)
        {
            if (!_sdBoot.ContainsKey(boot))
                _sdBoot.Add(boot, new Boot() { Name = boot });
            _sdBoot.TryGetValue(boot, out var bootObject);
            bootObject.Count++;
            bootObject.SetHours(hours);
            return bootObject;
        }

        private static void UpdateKnrmHelper(string redder, double hours, TypeInzet typeInzet, Boot boot)
        {
            if (string.IsNullOrWhiteSpace(redder) || string.Compare(redder, "n.v.t.", true) == 0 || int.TryParse(redder, out int ignore))
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

        private static void WriteResultFile()
        {
            using FileStream fs = File.Create("result" + DateTime.Now.Ticks + ".csv");
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
            var rowHeld = string.Format("{0};{1};{2};{3};", held.Name, held.Count, held.Hours.ToString("F1", CultureInfo.InvariantCulture).Replace('.', ','), held.HoursUp);
            foreach (var inzet in _sdInzet.Values)
            {
                if (held.SdInzet.TryGetValue(inzet.Name, out var inzetHeld))
                    rowHeld += string.Format("{0};{1};{2};", inzetHeld.Count, inzetHeld.Hours.ToString("F1", CultureInfo.InvariantCulture).Replace('.', ','), inzetHeld.HoursUp);
                else
                    rowHeld += "0;0;0;";
            }
            foreach (var inzet in _sdBoot.Values)
            {
                if (held.SdBoot.TryGetValue(inzet.Name, out var bootHeld))
                    rowHeld += string.Format("{0};{1};{2};", bootHeld.Count, bootHeld.Hours.ToString("F1", CultureInfo.InvariantCulture).Replace('.', ','), bootHeld.HoursUp);
                else
                    rowHeld += "0;0;0;";
            }
            return new UTF8Encoding(true).GetBytes(string.Format("{0}\r\n", rowHeld));
        }
    }
}