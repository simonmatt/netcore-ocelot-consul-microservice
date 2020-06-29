using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace Product.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly IConfiguration _configuration;

        public ProductsController(ILogger<ProductsController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Get()
        {
            string result =
                $"[Product Service] {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} --" +
                $"{Request.HttpContext.Connection.LocalIpAddress}:{_configuration["Consul:ServicePort"]}";
            return Ok(result);
        }
    }
}