using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ParentalControls.Common
{
    [ServiceContract]
    public interface ICommSvc
    {
        [OperationContract]
        void UpdateActiveWindow(string title);
    }
}
