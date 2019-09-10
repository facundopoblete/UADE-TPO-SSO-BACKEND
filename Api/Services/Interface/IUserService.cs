using System;
using System.Collections.Generic;
using DataAccess;

namespace Services.Interface
{
    public interface IUserService
    {
        User GetUser(Guid tenantId, string email);

        User GetUser(Guid tenantId, Guid userId);

        List<User> GetUsers(Guid tenantId);

        User CreateUser(Guid tenantId, string fullName, string email, string password);

        void DeleteUser(Guid tenantId, Guid userId);

        void UpdateUser(Guid tenantId, Guid userId, string fullName, string password, string extraClaims, string metadata);

        void RegisterUserEvent(Guid tenantId, Guid userId, string userEvent);
    }
}
