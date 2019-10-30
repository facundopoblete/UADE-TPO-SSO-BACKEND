using System.Linq;
using DataAccess;
using ManagementApi.Filters;
using ManagementApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace ManagementApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [JWTTenantFilter]
    public class MachineController : Controller
    {
        public IMachineService machineService;

        public MachineController(IMachineService machineService)
        {
            this.machineService = machineService;
        }

        /// <summary>
        /// Obtiene todas las machines del tenant.
        /// </summary>
        /// <returns></returns>
        /// <response code="401">El JWT no es valido.</response>
        [HttpGet]
        public IActionResult GetAllMachines()
        {
            Tenant tenant = RouteData.Values[JWTTenantFilter.TENANT_KEY] as Tenant;

            var machines = machineService.GetAllMachines(tenant.Id);

            return Ok(machines.Select(x => new MachineDTO
            {
                Id = x.Id,
                Name = x.Name,
                Secret = x.Secret
            }));
        }

        /// <summary>
        /// Crea una nueva Machine
        /// </summary>
        /// <param name="machine">Información de la nueva machine</param>
        /// <returns>La nueva machine</returns>
        /// <response code="401">El JWT no es valido.</response>
        [HttpPost]
        public IActionResult CreateMachine([FromBody]NewMachineDTO machine)
        {
            Tenant tenant = RouteData.Values[JWTTenantFilter.TENANT_KEY] as Tenant;

            var newMachine = machineService.AddMachine(tenant.Id, machine.Name);

            return Ok(new MachineDTO
            {
                Name = newMachine.Name,
                Secret = newMachine.Secret,
                Id = newMachine.Id
            });
        }
    }
}
