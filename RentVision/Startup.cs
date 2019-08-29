﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Piranha;
using Piranha.AspNetCore.Identity.SQLServer;
using Piranha.Manager;
using RentVision.Models;
using System.IO;

namespace RentVision
{
    public class Startup
    {
        /// <summary>
        /// The application config.
        /// </summary>
        public IConfiguration Configuration { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="configuration">The current configuration</param>
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLocalization(options =>
                options.ResourcesPath = "Resources"
            );
            services.AddMvc()
                .AddPiranhaManagerOptions()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddPiranha();
            services.AddPiranhaApplication();
            services.AddPiranhaFileStorage();
            services.AddPiranhaImageSharp();
            services.AddPiranhaManager();
            services.AddPiranhaMemoryCache();

            //
            // Setup Piranha & Asp.Net Identity with SQLite
            //
            //services.AddPiranhaEF(options =>
            //    options.UseSqlite(Configuration.GetConnectionString("piranha")));
            //services.AddPiranhaIdentityWithSeed<IdentitySQLiteDb>(options =>
            //    options.UseSqlite(Configuration.GetConnectionString("piranha")));

            //
            // Setup Piranha & Asp.Net Identity with SQL Server
            //
            services.AddPiranhaEF(options =>
                options.UseSqlServer(Configuration.GetConnectionString("piranha")));
            services.AddPiranhaIdentityWithSeed<IdentitySQLServerDb>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("piranha")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApi api)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Initialize Piranha
            App.Init(api);

            // Configure cache level
            App.CacheLevel = Piranha.Cache.CacheLevel.Basic;

            // Build content types
            var pageTypeBuilder = new Piranha.AttributeBuilder.PageTypeBuilder(api)
                .AddType(typeof(Models.BlogArchive))
                .AddType(typeof(Models.StandardPage))
                .AddType(typeof(Models.StartPage))
                .AddType(typeof(Models.LoginPage))
                .AddType(typeof(Models.RegisterPage));
            pageTypeBuilder.Build()
                .DeleteOrphans();
            var postTypeBuilder = new Piranha.AttributeBuilder.PostTypeBuilder(api)
                .AddType(typeof(Models.BlogPost));
            postTypeBuilder.Build()
                .DeleteOrphans();

            // Register middleware
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UsePiranha();
            app.UsePiranhaManager();
            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "areaRoute",
                    template: "{area:exists}/{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller=home}/{action=index}/{id?}");
            });

            // Custom manager styles
            App.Modules.Get<Module>().Styles.Add("~/css/components/block.css");

            // Custom manager scripts
            App.Modules.Get<Module>().Scripts.Add("~/assets/js/blocks/TwoColumnBlock.js");
            App.Modules.Get<Module>().Scripts.Add("~/assets/js/blocks/TwoColumnBlockGray.js");

            // Custom blocks
            App.Blocks.Register<TwoColumnBlock>();
            App.Blocks.Register<TwoColumnBlockGray>();
        }
    }
}
