using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProtoBuf.Grpc.Reflection;
using Server.Services;
using ProtoBuf.Grpc.Server;
using Server.Application;
using Server.Application.ChainOfResponsibilityUtils;
using Server.Domain.Games.Clicker;
using Server.Domain.Games.Meta;
using Server.Domain.Games.TheHat;
using Server.Domain.Lobby;
using Shared.Protos;

namespace Server
{
    public static class ServiceCollectionExtension
    {
        public static void AddMessageType<T>(this IServiceCollection services) where T : ClientMessage
        {
            services.AddSingleton<ChainHandlerManager<T>>();
            services.AddSingleton<IChainHandlerFactory>(p => p.GetService<ChainHandlerManager<T>>()!);
            services.AddSingleton<IClientEventEmitterResolver<T>>(p =>
                p.GetService<ChainHandlerManager<T>>()!);
            services.AddSingleton<SubscriptionManager<T>>();
        }
    }

    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole());
            services.AddCodeFirstGrpc();
            services.AddSingleton<Random>();
            services.AddSingleton<IGameFactory, ClickGameFactory>();
            services.AddSingleton<IGameFactory, HatGameFactory>();

            services.AddMessageType<PreGameClientMessage>();
            services.AddMessageType<LobbyClientMessage>();
            services.AddMessageType<InGameClientMessage>();
            services.AddSingleton<ChainResolver>();

            services.AddSingleton<LobbyFactory>();
            services.AddSingleton<GameFactoryResolver>();
            services.AddSingleton<ClientRouter>();
            services.AddSingleton<ClientInteractorFactory>();
            services.AddSingleton<PreGameClientFactory>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            System.IO.File.WriteAllText("protocol.proto.ignore",
                new SchemaGenerator().GetSchema(typeof(IMainServiceContract)));
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<ClientService>();

                endpoints.MapGet("/",
                    async context =>
                    {
                        await context.Response.WriteAsync(
                            "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                    });
            });
        }
    }
}