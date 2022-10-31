﻿using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.Extensions.Http;
using DTO;
using Models;
using IntrusionDetectionSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using IntrusionDetectionSystem.Models;


//Di, serilog, Settings 
namespace IntrusionDetectionSystem
{
    class Program
    {
    
        async static Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder(); 
            BuildConfig(builder); 

            Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Build())
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger(); 

            Log.Logger.Information("Application Starting"); 

            var host = Host.CreateDefaultBuilder()
                        .ConfigureServices((context, services) =>{
                            services.AddDbContext<EndpointDB>(opt => 
                            opt.UseNpgsql(@"Host=myserver;Username=mylogin;Password=mypass;Database=mydatabase"));
                            services.AddTransient<IStartup,Startup>();
                            services.AddTransient<IEndpoint,Endpoint>(); 
                            services.AddHttpClient<IStartup,Startup>(); 
                            services.AddAutoMapper(typeof(Program).Assembly);
                            services.AddScoped<IList<Connection>,List<Connection>>();

                        })
                        .UseSerilog()
                        .Build(); 
            
           /*  var svc = ActivatorUtilities.CreateInstance<Startup>(host.Services); 
            await svc.ProcessRepositories();
           var svc = ActivatorUtilities.CreateInstance<StartupPrometheusTest>(host.Services); 
            await svc.ProcessRepositories();*/
            var svc = ActivatorUtilities.CreateInstance<Endpoint>(host.Services); 
            await svc.Run(); 
        }

        static void BuildConfig(IConfigurationBuilder builder)
        {//Assembly.GetExecutingAssembly().Location
        //Path.GetDirectoryName(Directory.GetCurrentDirectory())
            builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional:false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")  ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables();
        }
    }
}




