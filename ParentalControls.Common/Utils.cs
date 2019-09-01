using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ParentalControls.Common
{
    public class Utils
    {
        public enum Span { Day = 0, Week = 1, WeekEnd = 2};
        public enum Sum { DaySum = 0, WeekSum = 1, WeekEndSum = 2};
        public enum Quota { DayQuota = 0, WeekQuota = 1, WeekEndQuota = 2 };

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowTextLength(IntPtr hWnd);
        public static readonly log4net.ILog log
      = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string GetCaptionOfActiveWindow()
        {
            var strTitle = string.Empty;
            var handle = GetForegroundWindow();
            // Obtain the length of the text   
            var intLength = GetWindowTextLength(handle) + 1;
            var stringBuilder = new StringBuilder(intLength);
            if (GetWindowText(handle, stringBuilder, intLength) > 0)
            {
                strTitle = stringBuilder.ToString();
            }
            return strTitle;
        }

        public static bool FirstRunThisDay()
        {
            DateTime lastRun = Convert.ToDateTime(ParentalControlsRegistry.GetRegistryKey().GetValue("LastStart"));
            DateTime now = DateTime.Now;
            return lastRun.Month != now.Month || lastRun.Day != now.Day;
        }

        public static bool IsWeekDay()
        {
            return (DateTime.Now.DayOfWeek != DayOfWeek.Saturday && DateTime.Now.DayOfWeek != DayOfWeek.Sunday);
        }

        public static bool BeforeSchoolDay()
        {
            return DateTime.Now.DayOfWeek != DayOfWeek.Saturday && DateTime.Now.DayOfWeek != DayOfWeek.Friday;
        }

        public static DayOfWeek GetCurrentDay()
        {
            DateTime time = DateTime.Now;
            return time.DayOfWeek;
        }

        public static decimal RestZeit(Span range,decimal uptime)
        {
            return Math.Round(
                Convert.ToDecimal(ParentalControlsRegistry.GetValue(range.ToString() + "Quota")) - 
                Convert.ToDecimal(ParentalControlsRegistry.GetValue(range.ToString() + "Sum")) - uptime,
                2);
        }
    }
}
