using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ParentalControls.Common
{
    [ServiceContract(SessionMode=SessionMode.Required, CallbackContract=typeof(IServiceCallback))]
    public interface ICommSvc
    {
        [OperationContract(IsOneWay =true)]
        void UpdateActiveWindow(string title);

    }

    public interface IServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void ExchangeData(string data);
    }
}
