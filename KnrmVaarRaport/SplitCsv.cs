namespace KnrmVaarRaport
{
    public static class SplitCsv
    {

        public static string[] Split(string line)
        {
            var result = new List<string>();
            int i = 0;
            while (true)
            {
                FindEmptyFields(ref line, result);
                FindFields(ref line, result);
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

        private static void FindFields(ref string line, List<string> result)
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
            result.Add(line.Substring(0, i).Trim('"'));
            if (line.Length > i)
                line = line.Substring(i);
            if (line.Length == i)
                line = string.Empty;
        }
    }
}