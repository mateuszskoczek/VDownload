namespace VDownload.Core.Services
{
    class StringC
    {
        #region MAIN

        // CUT STRING TO LENGTH
        public static string Cut(string str, int length)
        {
            string output = "";
            foreach (char c in str)
            {
                if (output.Length + 3 == length)
                {
                    return output + "...";
                }
                else
                {
                    output += c;
                }
            }
            return output;
        }


        // CONVERT BUMPER TO SNAKE CASE
        public static string BumperToSnakeCase(string str)
        {
            string output = str[0].ToString();
            foreach (char c in str)
            {
                output += (char.IsUpper(c) ? $"_{char.ToLower(c)}" : c);
            }
            return output;
        }

        #endregion
    }
}
