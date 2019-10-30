using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DataAccess;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace UsersApi.Utils
{
    public class JWTUtils
    {
        private const string ISSUER = "UADE SSO";
        private const string EXTRA_CLAIMS = "extra_claims";
        private const string TYPE = "entity_type";

        public static string CreateJWT(Tenant tenant, User user)
        {
            var signingKey = Encoding.UTF8.GetBytes(tenant.JwtSigningKey);
            var expiryDuration = tenant.JwtDuration;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = ISSUER,
                Audience = tenant.Id.ToString(),
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                Subject = new ClaimsIdentity(new List<Claim> { new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()) }),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(signingKey), SecurityAlgorithms.HmacSha256Signature)
            };

            if (expiryDuration > 0)
            {
                tokenDescriptor.Expires = DateTime.UtcNow.AddMinutes(expiryDuration);
            }

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtTokenHandler.CreateJwtSecurityToken(tokenDescriptor);

            if (user.ExtraClaims != null)
            {
                jwtToken.Payload.Add(EXTRA_CLAIMS, JsonConvert.DeserializeObject<dynamic>(user.ExtraClaims));
            }

            jwtToken.Payload.Add(TYPE, "user");

            var token = jwtTokenHandler.WriteToken(jwtToken);

            return token;
        }

        public static string CreateJWTMachine(Tenant tenant, Machine user)
        {
            var signingKey = Encoding.UTF8.GetBytes(tenant.JwtSigningKey);
            var expiryDuration = tenant.JwtDuration;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = ISSUER,
                Audience = tenant.Id.ToString(),
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                Subject = new ClaimsIdentity(new List<Claim> { new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()) }),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(signingKey), SecurityAlgorithms.HmacSha256Signature)
            };

            if (expiryDuration > 0)
            {
                tokenDescriptor.Expires = DateTime.UtcNow.AddMinutes(expiryDuration);
            }

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtTokenHandler.CreateJwtSecurityToken(tokenDescriptor);

            jwtToken.Payload.Add(TYPE, "machine");

            var token = jwtTokenHandler.WriteToken(jwtToken);

            return token;
        }
    }
}