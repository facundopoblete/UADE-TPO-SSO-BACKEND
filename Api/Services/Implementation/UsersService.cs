using System;
using DataAccess;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Services.Utils;
using Microsoft.EntityFrameworkCore;
using Services.Interface;
using Newtonsoft.Json.Linq;

namespace Services.Implementation
{
    public class UsersService : IUserService
    {
        DBContext dBContext = new DBContext();

        public User GetUser(Guid tenantId, string email)
        {
            var user = dBContext.User.FirstOrDefault(x => x.TenantId == tenantId && x.Email == email);

            dBContext.Entry(user).State = EntityState.Detached;
            dBContext.SaveChanges();

            return user;
        }

        public User GetUser(Guid tenantId, Guid userId)
        {
            var user = dBContext.User.Include(x => x.UserEvent).FirstOrDefault(x => x.TenantId == tenantId && x.Id == userId);

            dBContext.Entry(user).State = EntityState.Detached;
            dBContext.SaveChanges();

            return user;
        }

        public List<User> GetUsers(Guid tenantId)
        {
            return dBContext.User.Where(x => x.TenantId == tenantId).ToList();
        }

        public User CreateUser(Guid tenantId, string fullName, string email, string password)
        {
            var salt = PasswordUtils.GenerateSalt();

            var user = new User()
            {
                TenantId = tenantId,
                Id = Guid.NewGuid(),
                FullName = fullName,
                Email = email,
                PasswordSalt = Convert.ToBase64String(salt),
                PasswordHash = Convert.ToBase64String(PasswordUtils.GenerateSaltedHash(Encoding.UTF8.GetBytes(password), salt))
            };
           
            dBContext.Add(user);
            dBContext.SaveChanges();

            RegisterUserEvent(tenantId, user.Id, UserEvents.USER_CREATED);

            return user;
        }

        public void DeleteUser(Guid tenantId, Guid userId)
        {
            var events = dBContext.UserEvent.Where(x => x.Userid == userId && x.Tenantid == tenantId);

            dBContext.RemoveRange(events);
            dBContext.SaveChanges();

            var user = new User()
            {
                TenantId = tenantId,
                Id = userId
            };

            dBContext.Remove(user);
            dBContext.SaveChanges();
        }

        public void UpdateUser(Guid tenantId, Guid userId, string fullName, string password, string extraClaims, string metadata)
        {
            var existUser = this.GetUser(tenantId, userId);

            if (fullName != null)
            {
                existUser.FullName = fullName;
            }

            if (password != null)
            {
                var salt = PasswordUtils.GenerateSalt();
                existUser.PasswordSalt = Convert.ToBase64String(salt);
                existUser.PasswordHash = Convert.ToBase64String(PasswordUtils.GenerateSaltedHash(Encoding.UTF8.GetBytes(password), salt));
            }

            if (extraClaims != null)
            {
                var newClaims = JObject.Parse(extraClaims);
                var existClaims = JObject.Parse(existUser.ExtraClaims);

                newClaims.Merge(existClaims, new JsonMergeSettings()
                {
                    MergeArrayHandling = MergeArrayHandling.Union
                });

                existUser.ExtraClaims = newClaims.ToString();
            }

            if (metadata != null)
            {
                var newMetadata = JObject.Parse(metadata);
                var existMetadata = JObject.Parse(existUser.Metadata);

                newMetadata.Merge(existMetadata, new JsonMergeSettings()
                {
                    MergeArrayHandling = MergeArrayHandling.Union
                });

                existUser.Metadata = newMetadata.ToString();
            }

            dBContext.Update(existUser);
            dBContext.SaveChanges();

            RegisterUserEvent(tenantId, userId, UserEvents.USER_UPDATED);
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

            RegisterUserEvent(tenantId, userId, UserEvents.USER_METADATA_UPDATED);
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
