using DataAccess;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using UsersApi.Filters;
using UsersApi.Models;
using UsersApi.Utils;

namespace UsersApi.Controllers
{
    [Route("api")]
    public class MachineController : Controller
    {
        public IMachineService machineService;

        public MachineController(IMachineService machineService)
        {
            this.machineService = machineService;
        }

        /// <summary>
        /// Login de una machine
        /// </summary>
        /// <param name="login"></param>
        /// <returns>El JWT de machine</returns>
        /// <response code="401">Las credenciales no son validas.</response>
        [HttpPost("machine/login")]
        [TenantFilter]
        public IActionResult Login([FromBody] LoginMachineDTO login)
        {
            if (RouteData.Values[TenantFilter.TENANT_KEY] == null)
            {
                return Unauthorized();
            }

            Tenant tenant = RouteData.Values[TenantFilter.TENANT_KEY] as Tenant;

            var machine = machineService.GetMachine(tenant.Id, login.Id);

            if (machine == null)
            {
                return Unauthorized();
            }

            if (machine.Secret != login.Secret)
            {
                return Unauthorized();
            }

            return Ok(new TokenDTO
            {
                Token = JWTUtils.CreateJWTMachine(tenant, machine)
            });
        }
    }
}
