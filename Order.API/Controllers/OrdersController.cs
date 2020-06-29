using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ILogger<OrdersController> _logger;
        private readonly IConfiguration _configuration;

        public OrdersController(ILogger<OrdersController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Get()
        {
            string result =
                $"[Order Service] {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} --" +
                $"{Request.HttpContext.Connection.LocalIpAddress}:{_configuration["Consul:ServicePort"]}";
            return Ok(result);
        }
    }
}