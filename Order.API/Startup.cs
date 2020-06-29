using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Order.API.Helper;
using Order.API.Models;

namespace Order.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddDbContext<OrderDbContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("Default"));
            });

            services.AddCap(options =>
            {
                options.UseEntityFramework<OrderDbContext>();
                options.UseRabbitMQ(cfg =>
                {
                    cfg.HostName = "host.docker.internal";
                    //cfg.Port = 15672;
                    //cfg.UserName = "guest";
                    //cfg.Password = "guest";
                    //cfg.VirtualHost = "myvhost";
                });
                //options.UseDashboard();
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.RegisterConsul(Configuration, lifetime);
        }
    }
}