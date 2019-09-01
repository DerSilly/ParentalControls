using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParentalControls.Common
{
    public class ParentalControlsRegistry
    {

        public static RegistryKey GetRegistryKey(bool readOnly = false)
        {
            if(readOnly)
                return Registry.CurrentUser.OpenSubKey(@"SOFTWARE\SillyApps\Parental Controls");
            else
                return Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SillyApps\Parental Controls");
        }

        public static void IncrementKey(string key, Decimal incrementBy = 1)
        {
            ParentalControlsRegistry.GetRegistryKey().SetValue(key, Convert.ToDecimal(ParentalControlsRegistry.GetRegistryKey().GetValue(key)) + incrementBy);
        }

        public static string GetValue(string key, string defaultValue = "")
        {
            var result = GetRegistryKey().GetValue(key);
            return result==null?defaultValue:Convert.ToString(result);            
        }

        public static void SetValue(string key, string value)
        {
            GetRegistryKey().SetValue(key,value);
        }

        public static void SetValue(string key, decimal value)
        {
            GetRegistryKey().SetValue(key, value);
        }

        public static void SetValue(string key, DateTime value)
        {
            GetRegistryKey().SetValue(key, value);
        }
    }
}
