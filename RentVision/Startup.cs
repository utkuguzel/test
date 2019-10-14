using Microsoft.AspNetCore.Builder;
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
using RentVision.Models.Regions;
using System;
using System.IO;
using RentVision.Models.Configuration;
using Twinvision.Piranha.RentVision.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;
using System.Collections.Generic;
using reCAPTCHA.AspNetCore;

namespace RentVision
{
    public class Startup
    {
        /// <summary>
        /// The application config.
        /// </summary>
        public IConfiguration Configuration { get; set; }
        public static IConfiguration Config { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="configuration">The current configuration</param>
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
            Config = configuration;

            // Initialize Mollie keys in customerController
            var mollieSettings = Configuration.GetSection("MollieSettings");

            CustomerController.MollieKeyLive = mollieSettings["apiKeyLive"];
            CustomerController.MollieKeyTest = mollieSettings["apiKeyTest"];
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLocalization(options =>
                options.ResourcesPath = "Resources"
            );
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en");
                options.SupportedCultures = new List<CultureInfo>
                {
                    new CultureInfo("en"),
                    new CultureInfo("nl")
                };
            });

            // ReCaptcha
            services.Configure<RecaptchaSettings>(Configuration.GetSection("RecaptchaSettings"));
            services.AddTransient<IRecaptchaService, RecaptchaService>();

            services.AddMvc()
                .AddPiranhaManagerOptions()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddPiranha();
            services.AddPiranhaApplication();
            services.AddPiranhaFileStorage();
            services.AddPiranhaImageSharp();
            services.AddPiranhaManager();
            services.AddPiranhaMemoryCache();

            // Set API key as a default request header param since we need it for each call
            services.AddHttpClient("RentVisionApi", c =>
            {
                c.DefaultRequestHeaders.Add(
                    $"{Models.Configuration.Configuration.BackOffice.ApiKeyHeaderName}",
                    $"{Models.Configuration.Configuration.BackOffice.ApiKey}"
                );
            });

            services.AddSession();

            //
            // Setup Piranha & Asp.Net Identity with SQL Server
            //
            services.AddPiranhaEF(options =>
                options.UseSqlServer(Configuration.GetConnectionString("piranha")));
            services.AddPiranhaIdentityWithSeed<IdentitySQLServerDb>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("piranha")));

            //services.AddLogging(logging =>
            //    logging.AddFile("app.log", append: true));
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
                .AddType(typeof(BlogArchive))
                .AddType(typeof(StandardPage))
                .AddType(typeof(StartPage))
                .AddType(typeof(LoginPage))
                .AddType(typeof(RegisterPage))
                .AddType(typeof(ProductPage))
                .AddType(typeof(PlansPage))
                .AddType(typeof(SetupPage));

            pageTypeBuilder.Build()
                .DeleteOrphans();

            var postTypeBuilder = new Piranha.AttributeBuilder.PostTypeBuilder(api)
                .AddType(typeof(BlogPost));

            postTypeBuilder.Build()
                .DeleteOrphans();

            // Register middleware
            app.UseRequestLocalization();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UsePiranha();
            app.UsePiranhaManager();
            app.UseSession();
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
            App.Modules.Get<Piranha.Manager.Module>().Styles.Add("~/css/components/block.css");

            // Custom manager scripts
            App.Modules.Get<Piranha.Manager.Module>().Scripts.Add("~/assets/js/blocks/TwoColumnBlock.js");
            App.Modules.Get<Piranha.Manager.Module>().Scripts.Add("~/assets/js/blocks/TwoColumnBlockGray.js");

            App.Modules.Get<Piranha.Manager.Module>().Scripts.Add("~/assets/js/blocks/OneColumnBlock.js");
            App.Modules.Get<Piranha.Manager.Module>().Scripts.Add("~/assets/js/blocks/CallToActionBlock.js");

            // Custom blocks
            App.Blocks.Register<TwoColumnBlock>();
            App.Blocks.Register<TwoColumnBlockGray>();

            App.Blocks.Register<OneColumnBlock>();
            App.Blocks.Register<CallToActionBlock>(); // Optional
        }
    }
}
