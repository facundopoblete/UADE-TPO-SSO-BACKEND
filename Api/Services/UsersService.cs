using System;
using DataAccess;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Buffers.Text;
using System.Text;

namespace Services
{
    public class UsersService
    {
        DBContext dBContext = new DBContext();

        public User GetUser(Guid tenantId, string email)
        {
            return dBContext.User.FirstOrDefault(x => x.TenantId == tenantId && x.Email == email);
        }

        public User GetUser(Guid tenantId, Guid userId)
        {
            return dBContext.User.FirstOrDefault(x => x.TenantId == tenantId && x.Id == userId);
        }

        public List<User> GetUsers(Guid tenantId)
        {
            return dBContext.User.Where(x => x.TenantId == tenantId).ToList();
        }

        static byte[] GenerateSaltedHash(byte[] plainText, byte[] salt)
        {
            HashAlgorithm algorithm = new SHA256Managed();

            byte[] plainTextWithSaltBytes =
              new byte[plainText.Length + salt.Length];

            for (int i = 0; i < plainText.Length; i++)
            {
                plainTextWithSaltBytes[i] = plainText[i];
            }
            for (int i = 0; i < salt.Length; i++)
            {
                plainTextWithSaltBytes[plainText.Length + i] = salt[i];
            }

            return algorithm.ComputeHash(plainTextWithSaltBytes);
        }

        public bool IsValidPassword(User user, string password)
        {
            return Convert.ToBase64String(GenerateSaltedHash(Encoding.UTF8.GetBytes(password), Convert.FromBase64String(user.PasswordSalt))) == user.PasswordHash;
        }

        public User CreateUser(Guid tenantId, string fullName, string email, string password)
        {
            var rnd = new RNGCryptoServiceProvider();
            var buf = new byte[20];
            rnd.GetBytes(buf);

            var user = new User()
            {
                TenantId = tenantId,
                Id = Guid.NewGuid(),
                FullName = fullName,
                Email = email,
                PasswordSalt = Convert.ToBase64String(buf),
                PasswordHash = Convert.ToBase64String(GenerateSaltedHash(Encoding.UTF8.GetBytes(password), buf))
            };

            dBContext.Add(user);
            dBContext.SaveChanges();

            RegisterUserEvent(tenantId, user.Id, "USER CREATED");

            return user;
        }

        public void DeleteUser(Guid tenantId, Guid userId)
        {
            var user = new User()
            {
                TenantId = tenantId,
                Id = userId
            };

            dBContext.Remove(user);
            dBContext.SaveChanges();

            RegisterUserEvent(tenantId, userId, "USER DELETED");
        }

        public void UpdateUser(Guid tenantId, Guid userId, string fullName)
        {
            var user = new User()
            {
                TenantId = tenantId,
                Id = userId,
                FullName = fullName
            };

            dBContext.Update(user);
            dBContext.SaveChanges();

            RegisterUserEvent(tenantId, userId, "USER UPDATED");
        }

        public void UpdateUserMetadata(Guid tenantId, Guid userId, dynamic metadata)
        {
            var user = new User()
            {
                TenantId = tenantId,
                Id = userId,
                Metadata = metadata
            };

            dBContext.Update(user);
            dBContext.SaveChanges();

            RegisterUserEvent(tenantId, userId, "METADATA UPDATED");
        }

        public void RegisterUserEvent(Guid tenantId, Guid userId, string userEvent)
        {
            var newEvent = new UserEvent()
            {
                Tenantid = tenantId,
                Event = userEvent,
                When = DateTime.Now,
                Userid = userId
            };

            dBContext.Add(newEvent);
            dBContext.SaveChanges();
        }
    }
}
