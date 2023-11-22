using System.ServiceModel;

namespace RoutingServer
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        string GetItinary(string origin, string destination);

    }
}
