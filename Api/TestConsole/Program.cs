using System;
using Services;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            TenantsService s = new TenantsService();
            s.CreateTenant("prueba");
        }
    }
}
