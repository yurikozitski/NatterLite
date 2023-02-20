using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NatterLite.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using NatterLite.Services;
using NatterLite.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using NatterLite.Filters;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace NatterLite
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<User, IdentityRole>(opts =>
            {
                opts.Password.RequiredLength = 8;   
                opts.Password.RequireNonAlphanumeric = false;   
                opts.Password.RequireLowercase = false; 
                opts.Password.RequireUppercase = false; 
                opts.Password.RequireDigit = false;
                opts.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ'-@";
            }) 
                .AddEntityFrameworkStores<ApplicationContext>();
            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Account/Login");
            });
            services.AddSingleton<IPicturesProvider,DefaultUserPicturesProvider>();
            services.AddSingleton<ICountryList, GetCountryList>();
            services.AddSingleton<IImageValidator, ImageValidator>();
            services.AddSignalR();
            services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
            services.AddControllersWithViews();
            services.AddScoped<IsBannedFilter>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Chat}/{action=ChatMenu}");
                endpoints.MapHub<ChatHub>("/chat");
            });
        }
    }
}
