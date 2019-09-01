using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ParentalControls.Service
{
    static class Program
    {
        private static readonly ILog log = LogManager.GetLogger("log4net");
        static ServiceHost host = new ServiceHost(typeof(ParentalControls));
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            
            host.Open();
            log.Debug("Server is open");

            ServiceBase[] ServicesToRun;
            log4net.Config.XmlConfigurator.Configure();
            ServicesToRun = new ServiceBase[] 
            { 
                new ParentalControls() 
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
