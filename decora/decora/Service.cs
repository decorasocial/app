using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using RestSharp;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Collections;
using decora.Views;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;

namespace decora
{

    public static class RestClientExtensions
    {
        public static async Task<RestResponse> ExecuteAsync(this RestClient client, RestRequest request)
        {
            TaskCompletionSource<IRestResponse> taskCompletion = new TaskCompletionSource<IRestResponse>();
            RestRequestAsyncHandle handle = client.ExecuteAsync(request, r => taskCompletion.SetResult(r));
            return (RestResponse)(await taskCompletion.Task);
        }
    }

    class Service
    {

        public static string baseUrl = "https://951eneqrxf.execute-api.us-east-1.amazonaws.com/dev/";

        private static string key_header = "portal";
        private static string key_data = "portal_data";

        private static string Encoder(object obj, string type)
        {
            
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            string secret;

            switch (type)
            {
                case "header": secret = Service.key_header;
                    break;
                default: secret = Service.key_data;
                    break;
            };

            string token = encoder.Encode(obj, secret);

            return token;

        }

        public async static Task<dynamic> Run(string path, object call, string method, object parameters)
        {
            
            var token = Service.Encoder(call, "header");
            var token_data = Service.Encoder(parameters, "data");

            System.Diagnostics.Debug.Write("###################################################");
            System.Diagnostics.Debug.Write(token.ToString());
            System.Diagnostics.Debug.Write("###################################################");
            System.Diagnostics.Debug.Write(token_data.ToString());
            System.Diagnostics.Debug.Write("###################################################");

            dynamic json = JsonConvert.SerializeObject(true);

            var client = new RestClient(baseUrl);

            var mtd = Method.GET;

            switch (method.ToUpper())
            {
                case "POST":
                    mtd = Method.POST;
                    break;
                case "PUT":
                    mtd = Method.PUT;
                    break;
                case "DELETE":
                    mtd = Method.DELETE;
                    break;
                case "PATH":
                    mtd = Method.PATCH;
                    break;
                default:
                    mtd = Method.GET;
                    break;
            }


            System.Diagnostics.Debug.Write(mtd.ToString());
            System.Diagnostics.Debug.Write("###################################################");

            var request = new RestRequest(path, mtd);

            request.AddParameter("pkg", token_data.ToString());

            request.AddHeader("call", token.ToString());

            var response = await client.ExecuteAsync(request);

            var content = response.Content;
            
            json = JsonConvert.DeserializeObject<dynamic>(content);

            string outputJson = JsonConvert.SerializeObject(json);
            System.Diagnostics.Debug.Write(outputJson);
            System.Diagnostics.Debug.Write("###################################################");

            return json;

        }

        /*
        public async static Task<dynamic> Post(string endpoint, IDictionary<string, string> payload, string method, object parameters)
        {
            const string secret = "portal";
            const string secret_data = "portal_data";
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
            var token = encoder.Encode(payload, secret);
            var token_data = encoder.Encode(parameters, secret_data);

            dynamic json = JsonConvert.SerializeObject(true);

            var client = new RestClient(baseUrl);

            var request = new RestRequest(endpoint, Method.POST);

            request.AddParameter("pkg", token_data.ToString());

            request.AddHeader("call", token.ToString());

            // foreach (KeyValuePair<string, int> element in parameters)
            //    request.AddParameter(element.Key, element.Value);


            var response = await client.ExecuteAsync(request);

            var content = response.Content;


            json = JsonConvert.DeserializeObject<dynamic>(content);

            return json;
        }

        public async static Task<dynamic> PostInt(string endpoint, IDictionary<string, int> parameters, IDictionary<string, string> payload)
        {

            const string secret = "portal";
            const string secret_data = "portal_data";
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
            var token = encoder.Encode(payload, secret);
            var token_data = encoder.Encode(parameters, secret_data);

            dynamic json = JsonConvert.SerializeObject(true);

            var client = new RestClient(baseUrl);

            var request = new RestRequest(endpoint, Method.POST);

            request.AddParameter("pkg", token_data.ToString());

            request.AddHeader("call", token.ToString());

            // foreach (KeyValuePair<string, int> element in parameters)
            //    request.AddParameter(element.Key, element.Value);


            var response = await client.ExecuteAsync(request);

            var content = response.Content;
            

            json = JsonConvert.DeserializeObject<dynamic>(content);
           
            return json;

        }

    */

    }
}
