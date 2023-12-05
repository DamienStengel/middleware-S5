using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Proxy
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        Task<JCDStation> GetNearestStation(Position origin, Boolean isCheckingBikeAvailability);
    }
}
