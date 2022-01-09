namespace KnrmVaarRaport
{
    public static class SplitCsv
    {

        public static string[] Split(string line, StreamReader sr = null)
        {
            var result = new List<string>();
            int i = 0;
            while (true)
            {
                FindEmptyFields(ref line, result);
                FindFields(ref line, result, sr);
                if (line.Length == 0)
                    break;
                i++;
                if (i > 5000)
                    throw new Exception("Infinite loop detected");
            }
            return result.ToArray();
        }

        private static void FindEmptyFields(ref string line, List<string> result)
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
        }

        private static void FindFields(ref string line, List<string> result, StreamReader sr)
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
                if (!inQuote && c == ',')
                    break;
                i++;
            }
            if (line.Length == i && inQuote && sr != null)
            {
                line += sr.ReadLine();
                FindFields(ref line, result, sr);
                return;
            }


            result.Add(line.Substring(0, i).Trim('"'));
            if (line.Length > i)
                line = line.Substring(i);
            else if (line.Length == i)
                line = string.Empty;
        }

        public static string[] ToArray(string line)
        {
            var ret = line.Split(',');
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = ret[i].Trim('[', ']', '"', '"');
            }
            return ret;
        }
    }
}