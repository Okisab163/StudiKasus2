using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwittorApp.Data;
using TwittorApp.GraphQL;
using TwittorApp.Helpers;

namespace TwittorApp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private readonly IWebHostEnvironment _env;
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (_env.IsProduction())
            {
                Console.WriteLine("--> using sql server db");
                services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlServer(
                    Configuration.GetConnectionString("ProductionConnection")
                ));
            }
            else
            {
                Console.WriteLine("--> using local sql server db");
                services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlServer(
                    Configuration.GetConnectionString("localconnection")
                ));
            }

            services
                .AddGraphQLServer()
                .AddQueryType<Query>()
                .AddMutationType<Mutation>()
                .AddAuthorization()
                .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = _env.IsDevelopment()); ;

            services.Configure<TokenSettings>(Configuration.GetSection("TokenSettings"));

            services.Configure<KafkaSettings>(Configuration.GetSection("KafkaSettings"));

            services.AddAuthorization();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = Configuration.GetSection("TokenSettings").GetValue<string>("Issuer"),
                    ValidateIssuer = true,
                    ValidAudience = Configuration.GetSection("TokenSettings").GetValue<string>("Audience"),
                    ValidateAudience = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetSection("TokenSettings").GetValue<string>("Key"))),
                    ValidateIssuerSigningKey = true
                };
            });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGraphQL();
            });

            PrepDb.PrepPopulation(app, env.IsProduction());
        }
    }
}
