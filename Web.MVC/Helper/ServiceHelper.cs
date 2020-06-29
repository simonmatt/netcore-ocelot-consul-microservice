using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.Configuration;

namespace Web.MVC.Helper
{
    public class ServiceHelper : IServiceHelper
    {
        private readonly IConfiguration _configuration;
        private readonly ConsulClient _consulClient;

        private ConcurrentBag<string> _orderServiceUrls;
        private ConcurrentBag<string> _productServiceUrls;

        public ServiceHelper(IConfiguration configuration)
        {
            _configuration = configuration;
            _consulClient = new ConsulClient(c =>
            {
                c.Address = new Uri(_configuration["Consul:Address"]);
            });
        }
        public async Task<string> GetOrder()
        {
            // string[] serviceUrls = { "http://localhost:9060", "http://localhost:9061", "http://localhost:9062" };//订单服务的地址，可以放在配置文件或者数据库等等...

            //var consulClient = new ConsulClient(c =>
            //{
            //    c.Address = new Uri(_configuration["Consul:Address"]);
            //});

            // 每次随机访问一个服务实例
            // var Client = new RestClient(serviceUrls[new Random().Next(0, 3)]);
            // var request = new RestRequest("/api/orders", Method.GET);

            if (_orderServiceUrls == null)
            {
                return await Task.FromResult("[OrderService] is initializing instances...");
            }


            //var services = consulClient.Health.Service("OrderService", null, true, null).Result.Response;

            //string[] serviceUrls = services.Select(p => $"http://{p.Service.Address + ":" + p.Service.Port}").ToArray();

            //if (!serviceUrls.Any())
            //{
            //    return await Task.FromResult("[OrderService] instance list is null");
            //}

            // 每次随机访问一个服务实例
            var client = new RestClient(_orderServiceUrls.ElementAt(new Random().Next(0, _orderServiceUrls.Count)));
            var request = new RestRequest("/api/orders", method: Method.GET);

            var response = await client.ExecuteAsync(request, CancellationToken.None);
            return response.Content;
        }

        public async Task<string> GetProduct()
        {
            //string[] serviceUrls = { "http://localhost:9040", "http://localhost:9041", "http://localhost:9042" };//产品服务的地址，可以放在配置文件或者数据库等等...

            //var consulClient = new ConsulClient(c =>
            //{
            //    c.Address = new Uri(_configuration["Consul:Address"]);
            //});

            //var services = consulClient.Health.Service("ProductService", null, true, null).Result.Response;

            //string[] serviceUrls = services.Select(p => $"http://{p.Service.Address}:{p.Service.Port}").ToArray();

            //if (!serviceUrls.Any())
            //{
            //    return await Task.FromResult("[ProductService] instance list is null");
            //}

            if (_productServiceUrls == null)
            {
                return await Task.FromResult("[ProductService] is initializing instances...");
            }

            // 每次随机访问一个服务实例
            var Client = new RestClient(_productServiceUrls.ElementAt(new Random().Next(0, _productServiceUrls.Count)));
            var request = new RestRequest("/api/products", Method.GET);

            var response = await Client.ExecuteAsync(request, CancellationToken.None);
            return response.Content;
        }

        public void GetServices()
        {
            var serviceNames = new string[] { "OrderService", "ProductService" };

            Array.ForEach(serviceNames, p =>
            {
                Task.Run(() =>
                {
                    // WaitTime默认为5分钟
                    var queryOptions = new QueryOptions { WaitTime = TimeSpan.FromMinutes(10) };
                    while (true)
                    {
                        GetServices(queryOptions, p);
                    }
                });
            });
        }

        private void GetServices(QueryOptions queryOptions, string serviceName)
        {
            var res = _consulClient.Health.Service(serviceName, null, true, queryOptions).Result;

            Console.WriteLine(
                $"{DateTime.Now} retrieved {serviceName}: queryOptions.WaitIndex: {queryOptions.WaitIndex} #LastIndex: {res.LastIndex}");

            // 如果版本号不一致，说明服务列表变化了
            if (queryOptions.WaitIndex != res.LastIndex)
            {
                queryOptions.WaitIndex = res.LastIndex;

                // 服务列表
                var serviceUrls = res.Response.Select(p => $"http://{p.Service.Address}:{p.Service.Port}").ToArray();

                if (serviceName == "OrderService")
                {
                    _orderServiceUrls = new ConcurrentBag<string>(serviceUrls);
                }
                else if (serviceName == "ProductService")
                {
                    _productServiceUrls = new ConcurrentBag<string>(serviceUrls);
                }
            }
        }
    }
}