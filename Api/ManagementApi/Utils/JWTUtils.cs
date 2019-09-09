using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DataAccess;
using Microsoft.IdentityModel.Tokens;

namespace ManagementApi.Utils
{
    public class JWTUtils
    {
        private const string ISSUER = "UADE SSO";
        private const string TENANT_ID_CLAIM = "tenantId";
        public const string SSO_SECRET = "beb5c3a494ac42e39213804d71425eff";

        public static string CreateJWT(Tenant tenant)
        {
            var signingKey = Encoding.UTF8.GetBytes(SSO_SECRET);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = ISSUER,
                Audience = tenant.Id.ToString(),
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddHours(1),
                Subject = new ClaimsIdentity(new List<Claim> { new Claim(TENANT_ID_CLAIM, tenant.Id.ToString()) }),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(signingKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtTokenHandler.CreateJwtSecurityToken(tokenDescriptor);
            var token = jwtTokenHandler.WriteToken(jwtToken);

            return token;
        }
    }
}
