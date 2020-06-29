using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Product.API.Models;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Product.API.MessageDto;

namespace Product.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICapPublisher _capPublisher;
        private readonly ProductDbContext _productDbContext;

        public ProductsController(
            ILogger<ProductsController> logger,
            IConfiguration configuration,
            ICapPublisher capPublisher,
            ProductDbContext productDbContext)
        {
            _logger = logger;
            _configuration = configuration;
            _capPublisher = capPublisher;
            _productDbContext = productDbContext;
        }

        [HttpGet]
        public IActionResult Get()
        {
            string result =
                $"[Product Service] {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} --" +
                $"{Request.HttpContext.Connection.LocalIpAddress}:{_configuration["Consul:ServicePort"]}";
            return Ok(result);
        }

        [NonAction]
        [CapSubscribe("order.services.createorder")]
        public async Task ReduceStock(CreateOrderMessageDto message)
        {
            var product = await _productDbContext.Products.FirstOrDefaultAsync(p => p.ID == message.ProductID);
            product.Stock -= message.Count;

            Console.WriteLine($"Current stock: {product.Stock}");

            await _productDbContext.SaveChangesAsync();
        }
    }
}