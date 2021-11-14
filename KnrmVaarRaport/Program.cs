using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace KnrmVaarRaport
{
    public class Program
    {
        private static string _path = string.Empty;
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
                var sd = new SortedDictionary<string, KnrmHelden>();
                ReadFile(sd);
                AskToContinue();
                WriteResultFile(sd);
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }

        private static void ReadFile(SortedDictionary<string, KnrmHelden> sd)
        {
            using var sr = new StreamReader(_path);//"VAAR2021.csv"
            var i = 0;
            while (true)
            {
                var line = sr.ReadLine();
                if (i != 0)
                {
                    if (line == null) break;
                    var inzet = SplitCsv(line);
                    var isInzet = Regex.IsMatch(inzet[3], "^[0-9].*-");
                    Console.WriteLine(isInzet + " " + inzet[3]);
                    var hours = double.Parse(inzet[9].Replace(',', '.'));
                    var schipper = inzet[10];
                    var opstapper1 = inzet[11];
                    var opstapper2 = inzet[12];
                    var opstapper3 = inzet[13];
                    var opstapper4 = inzet[14];
                    var opstapper5 = inzet[15];
                    UpdateKnrmHelper(sd, schipper, isInzet, hours);
                    if (string.Compare(schipper, opstapper1) != 0)
                        UpdateKnrmHelper(sd, opstapper1, isInzet, hours);
                    if (!(schipper + opstapper1).Contains(opstapper2, StringComparison.CurrentCulture))
                        UpdateKnrmHelper(sd, opstapper2, isInzet, hours);
                    if (!(schipper + opstapper1 + opstapper2).Contains(opstapper3, StringComparison.CurrentCulture))
                        UpdateKnrmHelper(sd, opstapper3, isInzet, hours);
                    if (!(schipper + opstapper1 + opstapper2 + opstapper3).Contains(opstapper4, StringComparison.CurrentCulture))
                        UpdateKnrmHelper(sd, opstapper4, isInzet, hours);
                    if (!(schipper + opstapper1 + opstapper2 + opstapper3 + opstapper4).Contains(opstapper5, StringComparison.CurrentCulture))
                        UpdateKnrmHelper(sd, opstapper5, isInzet, hours);
                }
                i++;
            }
        }

        private static void UpdateKnrmHelper(SortedDictionary<string, KnrmHelden> sd, string redder, bool isInzet, double hours)
        {
            if (string.IsNullOrWhiteSpace(redder) || string.Compare(redder, "n.v.t.", true) == 0 || int.TryParse(redder, out int ignore))
                return;
            if (!sd.ContainsKey(redder))
                sd.Add(redder, new KnrmHelden(redder));
            sd.TryGetValue(redder, out var held);
            if (held != null)
            {
                if (isInzet)
                {
                    held.AddAction();
                    held.AddActionsHours(hours);
                }
                else
                {
                    held.AddTraining();
                    held.AddTrainingHours(hours);
                }
            }
        }

        private static void AskToContinue()
        {
            Console.Write("Druk op enter om het resultaat te verwerken ;)");
            while (Console.ReadKey().Key != ConsoleKey.Enter) { }
        }

        private static string[] SplitCsv(string line)
        {
            var result = new List<string>();
            line = line.Trim('"');
            var startIndex = 0;
            while (true)
            {
                var i = 0;
                while (string.Compare(line.Substring(0, 1), ",") == 0)
                {
                    line = line.Substring(1);
                    if (i > 0)
                        result.Add("");
                    if (line.Length == 0)
                        break;
                    i++;
                }
                var first = line.IndexOf('"', startIndex);
                if (first == -1) break;
                if (first > 0 && line.Length > first + 1 && string.Compare(line.Substring(first + 1, 1), ",") != 0)
                {
                    startIndex = first + 1;
                    continue;
                }
                startIndex = 0;
                var value = line.Substring(0, first);
                if (value.Length > 0 && string.Compare(value, ",") != 0)
                    result.Add(value);
                line = line.Substring(first + 1);
                if (line.Length == 0)
                    break;
            }
            return result.ToArray();
        }

        private static void WriteResultFile(SortedDictionary<string, KnrmHelden> sd)
        {
            using FileStream fs = File.Create("result" + DateTime.Now.Ticks + ".csv");
            Byte[] row = new UTF8Encoding(true).GetBytes("Redder,acties,actie uren,trainingen,training uren,totaal,totaal uren\r\n");
            fs.Write(row, 0, row.Length);
            foreach (var held in sd)
            {
                row = new UTF8Encoding(true).GetBytes(held.Value.Name + "," + held.Value.Actions + "," + held.Value.HoursActions.ToString("F1", CultureInfo.InvariantCulture) + "," + held.Value.Training + "," + held.Value.HoursTraining.ToString("F1", CultureInfo.InvariantCulture) + "," + held.Value.TotalActivities + "," + held.Value.HoursTotal.ToString("F1", CultureInfo.InvariantCulture) + "\r\n");
                fs.Write(row, 0, row.Length);
            }
        }
    }
}