using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISIC_FMT_MMCP_App
{
    public static class Extensions
    {
        public static byte[] GetBytes(this string input)
        {
            byte[] copy = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                copy[i] = (byte)input[i];
            }
            return copy;

            //byte[] copy = new byte[input.Length];
            //Array.Copy(input.ToCharArray(), copy, input.Length);
            //return copy;
        }

        public static String GetString(this byte[] input)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                sb.Append(input[i].ToString());
            }
            return sb.ToString();
        }

    }
}
