- Microservice
- Ocelot
- Docker
- Consul
- EventBus
- PostgreSql
- RabbitMQ
- CAP

> 参考
> - https://www.cnblogs.com/savorboard/p/cap.html
> - https://www.cnblogs.com/xhznl/p/13154851.html

### 集成`EventBus`

1. 从`docker`上拉取并运行`rabbitmq`镜像
```bash
# 拉取镜像
$ docker pull rabbitmq:management

# 运行镜像
$ docker run -d -p 15672:15672 -p 5672:5672 --name myrabbitmq rabbitmq:management
```
默认访问地址是 http://host.docker.internal，登录账号：guest，密码：guest

2. 分别向`Order.API`和`Product.API`项目添加以下Nuget包
   
```powershell
# ef & postgresql
$ dotnet add package Microsoft.EntityFrameworkCore
$ dotnet add package Microsoft.EntityFrameworkCore.Tools
$ dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL

# CAP
$ dotnet add package DotNetCore.CAP
$ dotnet add package DotNetCore.CAP.RabbitMQ
$ dotnet add package DotNetCore.CAP.PostgreSql

# optional
$ dotnet add package DotNetCore.Cap.Dashboard
```

3. 在`appsettings.json`文件中添加数据库连接字符串

```json
"ConnectionStrings": {
    "Default": "User ID=postgres;Password=123321;Host=host.docker.internal;Port=5432;Database=Order;Pooling=true;" 
  }
```

4. 在`Startup.cs`文件的`ConfigureServices`方法中添加依赖

```csharp

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
                });
            });
```

5. 在`Order.API`的控制器中新增以下代码

```csharp
# OrdersService.cs
        [Route("create")]
        public async Task<IActionResult> CreateOrder(Models.Order order)
        {
            using (var tran = _orderDbContext.Database.BeginTransaction(_capPublisher, autoCommit: true))
            {
                order.CreateTime=DateTime.Now;
                _orderDbContext.Orders.Add(order);

                var result = await _orderDbContext.SaveChangesAsync() > 0;

                if (result)
                {
                    // 发布下单事件
                    await _capPublisher.PublishAsync("order.services.createorder", new CreateOrderMessageDto
                    {
                        Count = order.Count,
                        ProductID = order.ProductID
                    });
                    return Ok();
                }

                return BadRequest();
            }
        }
```

6. 在`Product.API`的`ProductsController`中新增以下代码

```csharp
# ProductsController.cs
        [NonAction]
        [CapSubscribe("order.services.createorder")]
        public async Task ReduceStock(CreateOrderMessageDto message)
        {
            var product = await _productDbContext.Products.FirstOrDefaultAsync(p => p.ID == message.ProductID);
            product.Stock -= message.Count;

            Console.WriteLine($"Current stock: {product.Stock}");

            await _productDbContext.SaveChangesAsync();
        }
```

7. 在`Ocelot.APIGateway`项目中的`ocelot.json`文件中增加以下路由配置

```json
"DownstreamPathTemplate": "/api/orders/{url}",
"DownstreamScheme": "http",
"UpstreamPathTemplate": "/orders/{url}",
"UpstreamHttpMethod": [ "Get", "Post" ],
"ServiceName": "OrderService",
"FileCacheOptions": {
  "TtlSeconds": 5,
  "Region": "regionname"
}
```