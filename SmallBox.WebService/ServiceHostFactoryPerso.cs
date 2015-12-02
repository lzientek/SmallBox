using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using SmallBox.WebService.Models;

namespace SmallBox.WebService
{
    public class ServiceHostFactoryPerso : WebServiceHostFactory
    {
        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            var servicehost = new ServiceHost(typeof(JsonService), baseAddresses);
            //on ajoute mon behavior pour la gestion d'erreur perso
            servicehost.Description.Endpoints[0].Behaviors.Add(new WebHttpBehaviorEx());
            return servicehost;

        }
    }
}
