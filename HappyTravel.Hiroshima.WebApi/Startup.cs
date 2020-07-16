using System.Globalization;
using CacheFlow.Json.Extensions;
using FloxDc.CacheFlow.Extensions;
using FluentValidation.AspNetCore;
using HappyTravel.Geography;
using HappyTravel.Hiroshima.Common.Infrastructure;
using HappyTravel.Hiroshima.DirectContracts.Extensions;
using HappyTravel.Hiroshima.DirectManager.Extensions;
using HappyTravel.Hiroshima.WebApi.Infrastructure;
using HappyTravel.Hiroshima.WebApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetTopologySuite;
using Newtonsoft.Json;

namespace HappyTravel.Hiroshima.WebApi
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
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.WriteIndented = false;
                options.JsonSerializerOptions.IgnoreNullValues = true;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

            using var vaultClient = VaultHelper.CreateVaultClient(Configuration);
            vaultClient.Login(Configuration[Configuration["Vault:Token"]]).GetAwaiter().GetResult();
            var dbConnectionString = VaultHelper.GetDbConnectionString(vaultClient, "DirectContracts:Database:ConnectionOptions", "DirectContracts:Database:ConnectionString", Configuration);
            var redisEndpoint = Configuration[Configuration["Redis:Endpoint"]];
           
            services.AddDirectContractsServices(dbConnectionString);
            services.AddDirectManagerServices();
            
            services.AddSingleton(NtsGeometryServices.Instance.CreateGeometryFactory(GeoConstants.SpatialReferenceId));
            services.AddTransient<IAvailabilityService, AvailabilityService>();
          
            services.AddLocalization();
            services.AddOptions()
                .Configure<RequestLocalizationOptions>(options =>
                {
                    options.DefaultRequestCulture = new RequestCulture("en");
                    options.SupportedCultures = new[]
                    {
                        new CultureInfo("ar"),
                        new CultureInfo("en"),
                        new CultureInfo("ru")
                    };
                    options.RequestCultureProviders.Insert(0, new RouteDataRequestCultureProvider {Options = options});
                });

            services.AddHealthChecks()
                .AddCheck<ControllerResolveHealthCheck>(nameof(ControllerResolveHealthCheck));
            
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = false;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });
            
            services.AddResponseCompression()
                .AddHttpContextAccessor()
                .AddCors()
                .AddLocalization()
                .AddMemoryCache()
                .AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisEndpoint;
                })
                .AddDoubleFlow()
                .AddCacheFlowJsonSerialization();
                
            services.AddMvcCore()
                .AddControllersAsServices()
                .AddFormatterMappings()
                .AddApiExplorer()
                .AddNewtonsoftJson(options => { options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Unspecified; })
                .AddFluentValidation()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IOptions<RequestLocalizationOptions> localizationOptions)
        {
            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseRequestLocalization(localizationOptions.Value);
            app.UseRouting();
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyHeader());
            app.UseResponseCompression();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllers();
            });
        }
    }
}