using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace Order.API.Helper
{
    public static class ConsulHelper
    {
        public static IApplicationBuilder RegisterConsul(
            this IApplicationBuilder app,
            IConfiguration configuration,
            IHostApplicationLifetime lifetime)
        {
            var consulClient = new ConsulClient(c => { c.Address = new Uri(configuration["Consul:Address"]); });

            var registration = new AgentServiceRegistration()
            {
                ID = Guid.NewGuid().ToString(), //服务实例ID
                Name = configuration["Consul:ServiceName"],//服务名
                Address = configuration["Consul:ServiceIP"],//服务主机
                Port = Convert.ToInt32(configuration["Consul:ServicePort"]),//服务端口
                Check = new AgentServiceCheck
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),//服务启动多久后注册
                    Interval = TimeSpan.FromSeconds(10),//健康检查时间间隔

                    // 健康检查地址
                    HTTP =
                        $"http://{configuration["Consul:ServiceIP"]}:{configuration["Consul:ServicePort"]}{configuration["Consul:ServiceHealthCheck"]}",

                    // 超时时间
                    Timeout = TimeSpan.FromSeconds(60)
                }
            };

            // 注册服务
            consulClient.Agent.ServiceRegister(registration).Wait();

            // 应用程序终止时，取消注册
            lifetime.ApplicationStopping.Register(() =>
            {
                consulClient.Agent.ServiceDeregister(registration.ID).Wait();
            });

            return app;
        }
    }
}