using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckMethods
{
    public static class SimpleFuzzer
    {
        public static string RandomString(int length)
        {
            char[] chars = new char[length];
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                const int minValue = 0, maxValue = 25;
                int j = random.Next(minValue, maxValue);
                chars[i] = (char)('a' + j);
            };

            return new string(chars);
        }
    }
}
