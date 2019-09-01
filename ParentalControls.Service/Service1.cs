using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ParentalControls.Common;
using log4net;
using System.ServiceModel;

namespace ParentalControls.Service
{
    public partial class ParentalControls : ServiceBase, ICommSvc
    {
        private string m_ActiveWndTitle = "";
        private IServiceCallback callback = null;
        public ParentalControls()
        {
            InitializeComponent();
            this.AutoLog = true;
        }

        bool ShowBlocker()
        {
            Process p = Process.Start(ParentalControlsRegistry.GetRegistryKey(true).GetValue("Path") + @"\ParentalControls.GUI.exe");
            return p.Start();
        }

        public void UpdateActiveWindow(string title)
        {
            this.m_ActiveWndTitle = title;
        }

        Time GetCurrentTime()
        {
            DateTime dt = DateTime.Now;
            Time t = new Time();
            t.Hour = dt.Hour;
            t.Minutes = dt.Minute;
            t.Seconds = dt.Second;
            return t;
        }

        bool IsTimeToAlarm(Alarm alarm)
        {
            Time t = GetCurrentTime();
            return (alarm.AlarmTime.Hour == t.Hour && alarm.AlarmTime.Minutes == t.Minutes);
        }

        void WorkerThreadFunc()
        {
            try
            {
                /*AlarmsFile file = new AlarmsFile();
                file.FileName = (string)ParentalControlsRegistry.GetValue("AlarmFile");
                file.Add(new Alarm("Std", new Time(16, 15), DayOfWeek.Sunday, true));
                file.Save();*/
                while (!_shutdownEvent.WaitOne(0))
                {
                    decimal uptime = GetUptime();

                    //alle 5 Minuten (1/60*5*100 = 83)
                    if (uptime > 0 && Convert.ToInt32(uptime * 100)  % 83 == 0)
                    {
                        Utils.log.Debug("Restzeit Heute:           " + Utils.RestZeit(Utils.Span.Day, uptime));
                        Utils.log.Debug("Restzeit Woche:           " + Utils.RestZeit(Utils.Span.Week, uptime));
                        if (!Utils.IsWeekDay())
                        {
                            Utils.log.Debug("Restzeit Wochenende:      " + Utils.RestZeit(Utils.Span.WeekEnd, uptime));
                        }
                    }
                    callback.ExchangeData("test");
                
                    Utils.log.Debug("Aktives Fenster: " + m_ActiveWndTitle);

                    if (!Utils.BeforeSchoolDay() && 
                        DateTime.Now >= DateTime.Parse(DateTime.Now.ToString("dd.MM.yyyy ") + Properties.Settings.Default.FeierabendWochenEnde)) 
                    {
                        shutdown("After " + Properties.Settings.Default.FeierabendWochenEnde);
                        return;
                    }
                    else if (Utils.BeforeSchoolDay() && 
                        DateTime.Now >= DateTime.Parse(DateTime.Now.ToString("dd.MM.yyyy ") + Properties.Settings.Default.FeierabendWochenTag))
                    {
                        shutdown("After " + Properties.Settings.Default.FeierabendWochenTag);
                        return;
                    }

                    if (Utils.IsWeekDay() && 
                        Convert.ToDecimal(ParentalControlsRegistry.GetValue("DaySum")) + uptime > 
                        Convert.ToDecimal(ParentalControlsRegistry.GetValue("DayQuota")))
                    {
                        shutdown("DaySum > " + ParentalControlsRegistry.GetValue("DayQuota"));
                        return;
                    }
                    else if (!Utils.IsWeekDay() &&
                        Convert.ToDecimal(ParentalControlsRegistry.GetValue("WeekEndQuota","-1")) > 0 &&
                        Convert.ToDecimal(ParentalControlsRegistry.GetValue("WeekEndSum")) + uptime > 
                        Convert.ToDecimal(ParentalControlsRegistry.GetValue("WeekEndQuota")))
                    {
                        shutdown("WeekEndSum > " + ParentalControlsRegistry.GetValue("WeekEndQuota"));
                        return;
                    }
                    else if (Convert.ToDecimal(ParentalControlsRegistry.GetValue("WeekSum")) + uptime >
                             Convert.ToDecimal(ParentalControlsRegistry.GetValue("WeekQuota")))
                    {
                        shutdown("WeekSum > " + ParentalControlsRegistry.GetValue("WeekQuota"));
                        return;
                    }

                    /*if (file.IsValidForSaving())
                    {
                        foreach (Alarm alarm in file.Alarms)
                        {
                            if (alarm.RepeatDays.HasFlag(Utils.GetCurrentDay()) && IsTimeToAlarm(alarm))
                            {
                                if (ShowBlocker())
                                {
                                    Time a = GetCurrentTime();
                                    Console.WriteLine("The Alarm Blocker showed at {0}:{1} {2}.", a.Hour, a.Minutes, (a.Hour > 12 ? "PM" : "AM"));
                                }
                            }
                        }
                    }*/
                    Thread.Sleep(waitTime);
                }
            }
            catch(Exception e)
            {
                Utils.log.Debug(e.Message);
            }
        }

#if !DEBUG
       int waitTime = 60000;
#else
        int waitTime = 10000;
#endif
        
        protected void shutdown(string reason)
        {
            Utils.log.Debug("SHUTDOWN " + reason);
#if !DEBUG
         Process.Start("shutdown.exe", "/s /t 90");
#endif
        }

        ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
        Thread _thread;

        public DateTime LastActivationTime { get; private set; }



        private void init()
        {
            callback = OperationContext.Current.GetCallbackChannel<IServiceCallback>();
            DateTime now = DateTime.Now;
            Thread.Sleep(10000);
            ParentalControlsRegistry.SetValue("DayQuota", Properties.Settings.Default.TagesKontingent);
            ParentalControlsRegistry.SetValue("WeekQuota", Properties.Settings.Default.WochenKontingent);
            ParentalControlsRegistry.SetValue("WeekEndQuota", Properties.Settings.Default.WochenendKontingent);

            Utils.log.Debug("Diese Woche verbraucht:   " + ParentalControlsRegistry.GetValue("WeekSum"));
            Utils.log.Debug("Heute verbraucht:         " + ParentalControlsRegistry.GetValue("DaySum"));
            if (!Utils.IsWeekDay())
            {
                Utils.log.Debug("Am Wochenende verbraucht: " + ParentalControlsRegistry.GetValue("WeekEndSum"));
            }

            if (Utils.FirstRunThisDay())
            {
                ParentalControlsRegistry.SetValue("LastStart", now);
                ParentalControlsRegistry.SetValue("DaySum", "0,00");
                if (now.DayOfWeek == DayOfWeek.Monday)
                    ParentalControlsRegistry.SetValue("WeekSum", "0,00");
                else if (now.DayOfWeek == DayOfWeek.Saturday)
                    ParentalControlsRegistry.SetValue("WeekEndSum", "0,00");
            }
  
            _thread = new Thread(WorkerThreadFunc);
            _thread.Name = "ParentalControls.Worker";
            _thread.IsBackground = true;
            _thread.Start();

        }
        protected override void OnContinue()
        {
            Utils.log.Debug("[OnContinue]");
            this.LastActivationTime = DateTime.Now;
            init();
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            Utils.log.Debug("[OnPowerEvent] " + powerStatus.ToString());
            if(powerStatus.ToString().ToUpper().StartsWith("RESUME"))
            {
                this.LastActivationTime = DateTime.Now;
                init();
            }
            else if (powerStatus.ToString().ToUpper().StartsWith("SUSPEND"))
            {
                PersistSums();
            }
            return base.OnPowerEvent(powerStatus);
        }


        protected override void OnStart(string[] args)
        {
            Utils.log.Debug("[OnStart]");
            this.LastActivationTime = DateTime.Now;
            init();
        }

        protected override void OnPause()
        {
            Utils.log.Debug("[OnPause]");
            _thread.Abort();
            PersistSums();
            base.OnPause();
        }

        protected override void OnShutdown()
        {
            Utils.log.Debug("[OnShutdown]");
            _thread.Abort();
            PersistSums();
            base.OnShutdown();            
        }

        protected override void OnStop()
        {
            Utils.log.Debug("[OnStop]");
            PersistSums();
            _shutdownEvent.Set();
            if (!_thread.Join(3000))
            { 
                // give the thread 3 seconds to stop
                _thread.Abort();
            }
        }

        public Decimal GetUptime()
        {
            System.DateTime SystemStartTime = DateTime.Now.AddMilliseconds(-Environment.TickCount);

            TimeSpan uptime = DateTime.Now.ToUniversalTime() - this.LastActivationTime.ToUniversalTime();
            return Math.Round(Convert.ToDecimal(uptime.Hours) + Convert.ToDecimal(uptime.Minutes) / 60,2);
        }

        public void PersistSums()
        {
            Decimal uptime = GetUptime();

            ParentalControlsRegistry.IncrementKey("DaySum", uptime);
            ParentalControlsRegistry.IncrementKey("WeekSum", uptime);
            if (!Utils.IsWeekDay())
                ParentalControlsRegistry.IncrementKey("WeekEndSum", uptime);
        }

        public decimal GetTimeLeft(Utils.Span span)
        {
            return Utils.RestZeit(span, GetUptime());
        }
    }
}
