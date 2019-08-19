using Microsoft.AspNetCore.Mvc;

namespace UsersApi.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        [HttpPost("login")]
        public string Login()
        {
            return "login";
        }

        [HttpPost("signup")]
        public string Signup()
        {
            return "signup";
        }
    }
}
