using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Services.Implementation;
using Services.Interface;
using Swashbuckle.AspNetCore.Swagger;
using UsersApi.Filters;

namespace UsersApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddSwaggerGen(swaggerGen =>
            {
                swaggerGen.SwaggerDoc("v1", new Info { Title = "UADE SSO Users", Version = "v1" });

                var filePath = Path.Combine(AppContext.BaseDirectory, "UsersApi.xml");
                swaggerGen.IncludeXmlComments(filePath);

                swaggerGen.OperationFilter<TenantHeaderOperationFilter>();
                swaggerGen.OperationFilter<JWTOperationFilter>();
            });

            services.AddCors(o => o.AddPolicy("CorsPolicy", builder => {
                builder
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowAnyOrigin();
            }));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = false,
                    IssuerSigningKey = null,
                    IssuerSigningKeyResolver = (string token, SecurityToken securityToken, string kid, TokenValidationParameters validationParameters) =>
                    {
                        var jwt = new JwtSecurityToken(token);
                        var audience = jwt.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Aud);

                        if (audience == null)
                        {
                            throw new Exception("Token signature validation failed.");
                        }

                        TenantsService tenantsService = new TenantsService(new DBContext());
                        var tenant = tenantsService.GetTenant(Guid.Parse(audience.Value));

                        if (tenant == null)
                        {
                            throw new Exception("Token signature validation failed.");
                        }

                        List<SecurityKey> keys = new List<SecurityKey>();
                        
                        var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(tenant.JwtSigningKey));
                        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(hmac.Key), SecurityAlgorithms.HmacSha256Signature);
                        keys.Add(new SymmetricSecurityKey(hmac.Key));

                        return keys;
                    }
                };
            });

            services.AddScoped<ITenantService, TenantsService>();
            services.AddScoped<IUserService, UsersService>();

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseDeveloperExceptionPage();

            app.UseCors("CorsPolicy");

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "UADE SSO Users");
            });
        }
    }
}