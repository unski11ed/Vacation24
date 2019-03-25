using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using Vacation24.Models;
using Vacation24.Core.ExtensionMethods;
using System.Text;
using System.IO;
using Vacation24.Core.Configuration;
using Newtonsoft.Json;

namespace Vacation24.Core.Payment.Orders
{
    public interface IPaymentRequester
    {
        OrderResponse Execute(OrderRequest request);
    }

    public class PayURequester : IPaymentRequester
    {
        private PayUConfiguration configuration;

        public PayURequester(AppConfiguration configuration) {
            this.configuration = configuration.PayUConfiguration;
        }

        public OrderResponse Execute(OrderRequest request)
        {
            request.merchantPosId = this.configuration.User;
            return ExecuteJsonRequest<OrderRequest, OrderResponse>(
                configuration.ApiUrl,
                request
            );
        }

        private TOut ExecuteJsonRequest<TIn, TOut>(string url, TIn data)
        {
            var jsonData = JsonConvert.SerializeObject(data);
            var encoder = new UTF8Encoding();

            var requestObject = (HttpWebRequest)WebRequest.Create(configuration.ApiUrl);

            requestObject.ContentType = requestObject.Accept = "application/json";
            requestObject.UserAgent = "Custom PayU plugin 0.1";
            requestObject.Method = "Post";
            requestObject.ContentLength = encoder.GetByteCount(jsonData);
            requestObject.AllowAutoRedirect = false;

            requestObject.SetBasicAuthentication(configuration.User, configuration.Key);

            //Write content to stream
            using (var requestStreamWriter = new StreamWriter(requestObject.GetRequestStream(), encoder))
            {
                requestStreamWriter.Write(jsonData);
            }

            string outputContent = String.Empty;
            //Read response Content
            using (var responseObject = requestObject.GetResponseNoException())
            {
                using (var responseStreamReader = new StreamReader(responseObject.GetResponseStream(), new UTF8Encoding()))
                {
                    outputContent = responseStreamReader.ReadToEnd();

                    if(responseObject.StatusCode != HttpStatusCode.Accepted &&
                       responseObject.StatusCode != HttpStatusCode.Redirect)
                    {
                        throw new WebException(
                            "Pay U service Error", 
                            new Exception(
                                string.Format(
                                    "Server returned status code: {0} \n\t Content: {1}", 
                                    responseObject.StatusCode.ToString(), 
                                    outputContent
                                )
                            )
                        );
                    }
                }
            }

            return JsonConvert.DeserializeObject<TOut>(outputContent);
        }
    }
}