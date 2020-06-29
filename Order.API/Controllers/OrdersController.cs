using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Order.API.MessageDto;
using Order.API.Models;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ILogger<OrdersController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICapPublisher _capPublisher;
        private readonly OrderDbContext _orderDbContext;

        public OrdersController(
            ILogger<OrdersController> logger,
            IConfiguration configuration,
            ICapPublisher capPublisher,
            OrderDbContext orderDbContext)
        {
            _logger = logger;
            _configuration = configuration;
            _capPublisher = capPublisher;
            _orderDbContext = orderDbContext;
        }

        [HttpGet]
        public IActionResult Get()
        {
            string result =
                $"[Order Service] {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} --" +
                $"{Request.HttpContext.Connection.LocalIpAddress}:{_configuration["Consul:ServicePort"]}";
            return Ok(result);
        }

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
    }
}