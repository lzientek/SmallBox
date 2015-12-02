using System;
using System.Runtime.Serialization.Json;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using SmallBox.WebService.Models;

namespace SmallBox.WebService
{

    public class WebHttpBehaviorEx : WebHttpBehavior
    {
        protected override void AddServerErrorHandlers(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.ChannelDispatcher.ErrorHandlers.Clear();
            endpointDispatcher.ChannelDispatcher.ErrorHandlers.Add(new ErrorHandlerPerso());
        }
    }


    public class ErrorHandlerPerso : IErrorHandler
    {
        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            //creation du message d'erreur Json
            fault = Message.CreateMessage(version, "", new FaultModel() {ErrorCode = 400,FaultMessage = error.Message}, new DataContractJsonSerializer(typeof(FaultModel)));

            var wbf = new WebBodyFormatMessageProperty(WebContentFormat.Json);
            fault.Properties.Add(WebBodyFormatMessageProperty.Name, wbf);

            var rmp = new HttpResponseMessageProperty
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                StatusDescription = "An error occurs."
            };
            rmp.Headers.Add("Content-Type","application/json; charset=utf-8");
            fault.Properties.Add(HttpResponseMessageProperty.Name, rmp);
        }

        public bool HandleError(Exception error)
        {
            return true;
        }
    }

}
