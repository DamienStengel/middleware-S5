using System;
using RoutingServerApp.OpenRouteClasses;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace RoutingServer
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        List<Feature> GetItinary(string origin, string destination);
        [OperationContract]
        Boolean SendItineraryToQueue(string origin, string destination);
    }
}
