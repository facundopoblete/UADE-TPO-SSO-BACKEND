using Microsoft.AspNetCore.Mvc;

namespace UsersApi.Controllers
{
    [Route("api/[controller]")]
    public class HealthController : Controller
    {
        [HttpGet]
        public string Get()
        {
            return "pong";
        }
    }
}
