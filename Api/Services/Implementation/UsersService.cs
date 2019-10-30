using System;
using DataAccess;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Services.Utils;
using Microsoft.EntityFrameworkCore;
using Services.Interface;
using Newtonsoft.Json.Linq;
using System.Net.Mail;

namespace Services.Implementation
{
    public class UsersService : IUserService
    {
        public DBContext dBContext;
        private IEmailSenderService emailSenderService;

        public UsersService(DBContext dBContext, IEmailSenderService emailSenderService)
        {
            this.dBContext = dBContext;
            this.emailSenderService = emailSenderService;
        }

        public User GetUser(Guid tenantId, string email)
        {
            var user = dBContext.User.FirstOrDefault(x => x.TenantId == tenantId && x.Email == email);

            return user;
        }

        public User GetUser(Guid tenantId, Guid userId)
        {
            var user = dBContext.User.Include(x => x.UserEvent).FirstOrDefault(x => x.TenantId == tenantId && x.Id == userId);

            return user;
        }

        public List<User> GetUsers(Guid tenantId)
        {
            return dBContext.User.Where(x => x.TenantId == tenantId).ToList();
        }

        public User CreateUser(Guid tenantId, string fullName, string email, string password)
        {
            MailAddress emailAddress;

            var existUser = this.GetUser(tenantId, email);

            if (existUser != null)
            {
                throw new Exception("User already exists");
            }

            try
            {
                emailAddress = new MailAddress(email);
            }
            catch (FormatException e)
            {
                throw e;
            }

            var salt = PasswordUtils.GenerateSalt();

            var user = new User()
            {
                TenantId = tenantId,
                Id = Guid.NewGuid(),
                FullName = fullName,
                Email = emailAddress.Address,
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
            var events = dBContext.UserEvent.Where(x => x.Userid == userId && x.Tenantid == tenantId).AsNoTracking();

            dBContext.UserEvent.RemoveRange(events);

            var user = dBContext.User.Where(x => x.Id == userId && x.TenantId == tenantId).FirstOrDefault();

            dBContext.User.Remove(user);
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

                if (newClaims.Children().Count() == 0)
                {
                    existUser.ExtraClaims = newClaims.ToString();
                }
                else
                {
                    var existClaims = JObject.Parse(existUser.ExtraClaims ?? "{}");

                    existClaims.Merge(newClaims, new JsonMergeSettings()
                    {
                        MergeArrayHandling = MergeArrayHandling.Union
                    });

                    existUser.ExtraClaims = existClaims.ToString();
                }
            }

            if (metadata != null)
            {
                var newMetadata = JObject.Parse(metadata);

                if (newMetadata.Children().Count() == 0)
                {
                    existUser.Metadata = newMetadata.ToString();
                }
                else
                {
                    var existMetadata = JObject.Parse(existUser.Metadata ?? "{}");

                    existMetadata.Merge(newMetadata, new JsonMergeSettings()
                    {
                        MergeArrayHandling = MergeArrayHandling.Union
                    });

                    existUser.Metadata = existMetadata.ToString();
                }
            }

            dBContext.Update(existUser);
            dBContext.SaveChanges();

            RegisterUserEvent(tenantId, userId, UserEvents.USER_UPDATED);
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

        public void UserForgotPassword(Guid tenantId, string userEmail)
        {
            var user = this.GetUser(tenantId, userEmail);

            if (user == null)
            {
                return;
            }

            RecoverPassword recoverPasswordEntry = new RecoverPassword()
            {
                TenantId = tenantId,
                UserId = user.Id,
                IsValid = true
            };

            this.dBContext.Add(recoverPasswordEntry);
            this.dBContext.SaveChanges();

            var toAddress = new MailAddress(user.Email, user.FullName);
            
            const string subject = "Password Reset";
            string body = String.Format("https://uade-sso-login.herokuapp.com/recover?id={0}", recoverPasswordEntry.Id);

            if (emailSenderService != null)
            {
                this.emailSenderService.sendEmail(toAddress, subject, body);
            }
        }

        public void ChangeUserPassword(Guid tenantId, Guid userId, string newPassword)
        {
            var user = this.GetUser(tenantId, userId);

            if (user == null)
            {
                throw new Exception();
            }

            var salt = PasswordUtils.GenerateSalt();
            user.PasswordSalt = Convert.ToBase64String(salt);
            user.PasswordHash = Convert.ToBase64String(PasswordUtils.GenerateSaltedHash(Encoding.UTF8.GetBytes(newPassword), salt));

            dBContext.Update(user);
            dBContext.SaveChanges();
        }


        public bool ChangeUserPasswordFromRecover(Guid tenantId, Guid RecoverId, string newPassword)
        {
            var recoveryPassword = dBContext.RecoverPassword.FirstOrDefault(x => x.TenantId == tenantId && x.Id == RecoverId && x.IsValid == true);

            if (recoveryPassword == null)
            {
                return false;
            }

            this.ChangeUserPassword(tenantId, recoveryPassword.UserId, newPassword);

            return true;
        }

        private static string CreateRandomPassword(int length = 8)
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
