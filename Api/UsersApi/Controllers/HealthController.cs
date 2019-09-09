using Microsoft.AspNetCore.Mvc;

namespace UsersApi.Controllers
{
    [Route("api/[controller]")]
    public class HealthController : Controller
    {
        /// <summary>
        /// Ping para validar si el servicio esta activo
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public string Get() => "pong";
    }
}
