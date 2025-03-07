namespace KnrmVaarRaport
{
    public static class SplitCsv
    {

        public static string[] Split(string line, StreamReader sr = null, char separator = ',')
        {
            var result = new List<string>();
            int i = 0;
            while (true)
            {
                FindEmptyFields(ref line, result, separator);
                FindFields(ref line, result, sr, separator);
                if (line.Length == 0)
                    break;
                i++;
                if (i > 5000)
                    throw new Exception("Infinite loop detected");
            }
            return result.ToArray();
        }

        private static void FindEmptyFields(ref string line, List<string> result, char separator)
        {
            var i = 0;
            while (string.CompareOrdinal(line.Substring(0, 1),separator.ToString()) == 0)
            {
                line = line.Substring(1);
                if (i > 0)
                    result.Add("");
                if (line.Length == 0)
                    break;
                i++;
            }
        }

        private static void FindFields(ref string line, List<string> result, StreamReader sr, char separator)
        {
            var inQuote = false;
            int i = 0;
            foreach (var c in line)
            {
                if (c == '"')
                {
                    inQuote = !inQuote;
                    i++;
                    continue;
                }
                if (!inQuote && c == separator)
                    break;
                i++;
            }
            if (line.Length == i && inQuote && sr != null)
            {
                line += sr.ReadLine();
                FindFields(ref line, result, sr, separator);
                return;
            }


            result.Add(line.Substring(0, i).Trim('"'));
            if (line.Length > i)
                line = line.Substring(i);
            else if (line.Length == i)
                line = string.Empty;
        }

        public static string[] ToArray(string line, char separator)
        {
            var ret = line.Split(separator);
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = ret[i].Trim('[', ']', '"', '"');
            }
            return ret;
        }
    }
}