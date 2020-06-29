using System.Threading;
using System.Threading.Tasks;
using RestSharp;

namespace Web.MVC.Helper
{
    public class GatewayServiceHelper : IServiceHelper
    {
        public async Task<string> GetOrder()
        {
            var client=new RestClient("http://localhost:9070");
            var request=new RestRequest("/orders",Method.GET);

            var response = await client.ExecuteAsync(request, CancellationToken.None);
            return response.Content;
        }

        public async Task<string> GetProduct()
        {
            var client = new RestClient("http://localhost:9070");
            var request = new RestRequest("/products", Method.GET);

            var response = await client.ExecuteAsync(request, CancellationToken.None);
            return response.Content;
        }

        public void GetServices()
        {
            throw new System.NotImplementedException();
        }
    }
}