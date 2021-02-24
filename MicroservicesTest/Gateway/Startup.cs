using Gateway.Infrastructure;
using Gateway.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway {
    public class Startup {
        IConfiguration Configuration;
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services) {
            services.Configure<List<Route>>(Configuration.GetSection($"{nameof(Route)}s"));
            services.Configure<List<Route>>(Configuration.GetSection($"authenticationService"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<List<Route>> routes, IOptions<Destination> authenticationService) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            Router router = new Router(routes.Value, authenticationService.Value);
            app.UseEndpoints(endpoints => {
                endpoints.MapGet("/", async context => {
                    await context.Response.WriteAsync("Hello World!");
                });

                if (router.Routes.Count > 0) {
                    foreach (var r in router.Routes) {
                        endpoints.MapGet(r.Endpoint, async context => {
                            await context.Response.WriteAsync(await router.ProcessRequest(context.Request));
                        });
                    }
                }
            });
        }
    }
}
