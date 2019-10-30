using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess;
using Services.Interface;

namespace Services.Implementation
{
    public class MachineService : IMachineService
    {
        public DBContext dBContext;

        public MachineService(DBContext dBContext, IEmailSenderService emailSenderService)
        {
            this.dBContext = dBContext;
        }

        public Machine AddMachine(Guid tenantId, string name)
        {
            var newMachine = new Machine()
            {
                TenantId = tenantId,
                Name = name,
                Id = Guid.NewGuid(),
                Secret = "asd"
            };

            dBContext.Machine.Add(newMachine);
            dBContext.SaveChanges();

            return newMachine;
        }

        public void DeleteMachine(Guid tenantId, Guid id)
        {
            throw new NotImplementedException();
        }

        public List<Machine> GetAllMachines(Guid tenantId)
        {
            return dBContext.Machine.Where(x => x.TenantId == tenantId).ToList();
        }

        public Machine GetMachine(Guid tenantId, Guid id)
        {
            return dBContext.Machine.FirstOrDefault(x => x.TenantId == tenantId && x.Id == id);
        }
    }
}
