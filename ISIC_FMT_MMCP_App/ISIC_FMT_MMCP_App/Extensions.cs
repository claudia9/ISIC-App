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

        /*public static async Task<ICharacteristic> GetCharacteristic(this IDevice device)
        {
            Guid WRITE_SERVICE = Guid.Parse("0000ffe0-0000-1000-8000-00805f9b34fb");
            Guid WRITE_CHARACTERISTIC = Guid.Parse("0000ffe1-0000-1000-8000-00805f9b34fb");


            IService writeService = await device.GetServiceAsync(WRITE_SERVICE);
            System.Diagnostics.Debug.WriteLine("Write service found: " + writeService.ToString());

            ICharacteristic writeCharacteristic = writeService.GetCharacteristicAsync(WRITE_CHARACTERISTIC);
            Debug.WriteLine("Write characteristic found: " + writeCharacteristic.ToString());
            return writeCharacteristic;
        }*/

    }
}