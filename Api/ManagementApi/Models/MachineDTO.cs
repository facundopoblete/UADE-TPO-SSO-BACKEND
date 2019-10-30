using System;
namespace ManagementApi.Models
{
    public class MachineDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Secret { get; set; }
    }
}
