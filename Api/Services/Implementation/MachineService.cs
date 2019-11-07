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
                Secret = createRandomValue(50)
            };

            dBContext.Machine.Add(newMachine);
            dBContext.SaveChanges();

            return newMachine;
        }

        public void DeleteMachine(Guid tenantId, Guid id)
        {
            var machine = dBContext.Machine.Where(x => x.TenantId == tenantId && x.Id == id).FirstOrDefault();

            if (machine == null)
            {
                return;
            }

            dBContext.Machine.Remove(machine);
            dBContext.SaveChanges();
        }

        public List<Machine> GetAllMachines(Guid tenantId)
        {
            return dBContext.Machine.Where(x => x.TenantId == tenantId).ToList();
        }

        public Machine GetMachine(Guid tenantId, Guid id)
        {
            return dBContext.Machine.FirstOrDefault(x => x.TenantId == tenantId && x.Id == id);
        }

        private static string createRandomValue(int length = 8)
        {
            string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
            Random random = new Random();

            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }

            return new string(chars);
        }
    }
}
