using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Extensions
{
    public static class StringExtensions
    {
        public static string CreateRandom(int length) => CreateRandom(length, "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm1234567890`~!@#$%^&*()-_=+[{]};:'\"\\|,<.>/?");
        public static string CreateRandom(int length, IEnumerable<char> characters) => new string(Enumerable.Repeat(characters, length).Select(s => s.ElementAt(Random.Shared.Next(s.Count()))).ToArray());

        public static string Shuffle(this string str)
        {
            char[] array = str.ToCharArray();
            Random rng = Random.Shared;
            int n = array.Length;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                char value = array[k];
                array[k] = array[n];
                array[n] = value;
            }
            return new string(array);
        }
    }
}
