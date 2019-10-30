using System;
using System.Collections.Generic;
using DataAccess;

namespace Services.Interface
{
    public interface IMachineService
    {
        Machine AddMachine(Guid tenantId, string name);

        void DeleteMachine(Guid tenantId, Guid id);

        List<Machine> GetAllMachines(Guid tenantId);

        Machine GetMachine(Guid tenantId, Guid id);
    }
}
