using log4net;
using ParentalControls.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParentalControls.Service
{
    static class Program
    {
        static ServiceHost host;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ParentalControls pc;

            ServiceBase[] ServicesToRun;
            log4net.Config.XmlConfigurator.Configure();
            Thread.Sleep(6000);

            try
            {
                Utils.log.Debug("Start Service");
                pc = new ParentalControls();
                ServicesToRun = new ServiceBase[]
                {
                    pc
                };
                
                host = new ServiceHost(pc);
                host?.Open();
                Utils.log.Debug("Server is open");
                /*foreach (ServiceEndpoint se in host.Description.Endpoints)
                {
                    Trace.WriteLine(se.Address);
                    // LogMessage(se.Address.ToString());
                }*/
                

                ServiceBase.Run(ServicesToRun);
            }
            catch (Exception e)
            {
                Utils.log.Error(e.Message);
            }            
        }
    }
}
