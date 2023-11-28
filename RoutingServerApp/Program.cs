// add the WCF ServiceModel namespace 
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace RoutingServer
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Uri httpUrl = new Uri("http://localhost:8090/RoutingServer/");
            using (ServiceHost host = new ServiceHost(typeof(Service), httpUrl))
            {
                // Créer et configurer un BasicHttpBinding sans sécurité
                BasicHttpBinding binding = new BasicHttpBinding();
                binding.Security.Mode = BasicHttpSecurityMode.None;
                ServiceDebugBehavior debugBehavior = host.Description.Behaviors.Find<ServiceDebugBehavior>();
                if (debugBehavior == null)
                {
                    debugBehavior = new ServiceDebugBehavior();
                    host.Description.Behaviors.Add(debugBehavior);
                }
                debugBehavior.IncludeExceptionDetailInFaults = true;

                // Ajouter le point de terminaison
                host.AddServiceEndpoint(typeof(IService), binding, "");

                // Activer la publication de métadonnées, si nécessaire
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior { HttpGetEnabled = true };
                host.Description.Behaviors.Add(smb);

                // Démarrer le service
                host.Open();

                Console.WriteLine("Service is hosted at " + DateTime.Now.ToString());
                Console.WriteLine("Routing Server is running... Press <Enter> key to stop");
                Console.ReadLine();

                // Le service sera fermé lorsque l'utilisateur appuie sur Entrée
                host.Close();

            }
        }
    }
}
