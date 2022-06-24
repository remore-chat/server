using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remore.Server.EF;
using Remore.Server.Services;

namespace Remore.Server
{
    public static class ServiceContainer
    {

        private static IHost _host = Host
           .CreateDefaultBuilder()
            .ConfigureLogging((context, logging) => {
                var env = context.HostingEnvironment;
                var config = context.Configuration.GetSection("Logging");
                logging.AddConsole();
                logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", Microsoft.Extensions.Logging.LogLevel.None);
                logging.AddFilter("Microsoft.EntityFrameworkCore.Model.Validation", Microsoft.Extensions.Logging.LogLevel.None);
                logging.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", Microsoft.Extensions.Logging.LogLevel.None);
                logging.AddFilter("Microsoft.EntityFrameworkCore.Query", Microsoft.Extensions.Logging.LogLevel.None);
            })
           .ConfigureServices((context, services) =>
           {
               services.AddDbContext<ServerDbContext>();
               services.AddTransient<ConfigurationService>();
               var mapperConfig = new MapperConfiguration(mc =>
               {
                   mc.AddProfile(new MappingProfile());
               });
               IMapper mapper = mapperConfig.CreateMapper();
               services.AddSingleton(mapper);
           })
           .Build();

        public static T GetService<T>()
            where T : class
            => _host.Services.GetService(typeof(T)) as T;
    }
}
