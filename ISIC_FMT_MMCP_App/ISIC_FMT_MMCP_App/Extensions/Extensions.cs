using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ISIC_FMT_MMCP_App
{
    public static class Extensions
    {

        public static String GetString(this byte[] bArr)
        {
            return Encoding.UTF8.GetString(bArr, 0, bArr.Length);
        }
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

        /*public static String GetHexString(this byte[] input)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                sb.Append(input[i].ToString("0x02x"));
            }
            return sb.ToString();
        }*/

        public static String GetHexString(this byte[] bArr, char separator = ' ')
        {
            StringBuilder sb = new StringBuilder();
            if (bArr == null)
            {
                return String.Empty;
            }
            if (separator == '\0')
            {
                foreach (byte b in bArr)
                {
                    sb.AppendFormat("{0:X2}", b);
                }
            }
            else
            {
                foreach (byte b in bArr)
                {
                    sb.AppendFormat("{0:X2}{1}", b, separator);
                }
            }
            return sb.ToString();
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

    }
}