using Microsoft.AspNetCore.Mvc;

namespace Product.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("OK");
        }
    }
}