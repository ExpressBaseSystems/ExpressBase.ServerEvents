using ExpressBase.Common;
using ExpressBase.Objects.ServiceStack_Artifacts;
using Funq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Logging;
using ServiceStack.Redis;
using System;

namespace ExpressBase.ServerEvents
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection(opts =>
            {
                opts.ApplicationDiscriminator = "expressbase.serverevents";
            });
            // Add framework services.
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseServiceStack(new AppHost());
        }
    }

    public class AppHost : AppHostBase
    {
        public AppHost() : base("EXPRESSbase Server Events", typeof(AppHost).Assembly) { }

        public override void Configure(Container container)
        {
            LogManager.LogFactory = new ConsoleLogFactory(debugEnabled: true);

            var jwtprovider = new JwtAuthProviderReader
            {
                HashAlgorithm = "RS256",
                PublicKeyXml = Environment.GetEnvironmentVariable(EnvironmentConstants.EB_JWT_PUBLIC_KEY_XML),
#if (DEBUG)
                RequireSecureConnection = false,
#endif
                PersistSession = true,
                SessionExpiry = TimeSpan.FromHours(12)
            };

            this.Plugins.Add(new CorsFeature(allowedHeaders: "Content-Type, Authorization, Access-Control-Allow-Origin, Access-Control-Allow-Credentials"));
            this.Plugins.Add(new ServerEventsFeature());
            this.Plugins.Add(new AuthFeature(() => new CustomUserSession(),
                new IAuthProvider[] {
                    jwtprovider,
                }));

#if (DEBUG)
            SetConfig(new HostConfig { DebugMode = true });
#endif
            SetConfig(new HostConfig { DefaultContentType = MimeTypes.Json });

            var redisConnectionString = string.Format("redis://{0}@{1}:{2}",
               Environment.GetEnvironmentVariable(EnvironmentConstants.EB_REDIS_PASSWORD),
               Environment.GetEnvironmentVariable(EnvironmentConstants.EB_REDIS_SERVER),
               Environment.GetEnvironmentVariable(EnvironmentConstants.EB_REDIS_PORT));

            container.Register<IRedisClientsManager>(c => new RedisManagerPool(redisConnectionString));
            container.Register<IUserAuthRepository>(c => new RedisAuthRepository(c.Resolve<IRedisClientsManager>()));
            container.Register<IServerEvents>(c => new RedisServerEvents(c.Resolve<IRedisClientsManager>()));
            container.Resolve<IServerEvents>().Start();
        }
    }
}