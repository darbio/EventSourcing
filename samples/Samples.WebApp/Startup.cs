using JKang.EventSourcing.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Samples.Domain;
using Samples.Persistence;
using Samples.WebApp.Data;
using System;

namespace Samples.WebApp
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services
                .AddScoped<IGiftCardRepository, GiftCardRepository>()
                ;

            services
                .AddDbContext<SampleDbContext>(x =>
                {
                    x.UseInMemoryDatabase("eventstore");
                });

            services
                .AddDefaultAWSOptions(Configuration.GetAWSOptions())
                .AddEventSourcing(builder =>
                {
                    builder
                        .UseJsonEventSerializer()
                        //.UseTextFileEventStore<GiftCard, Guid>(x =>
                        //{
                        //    x.Folder = "C:\\Temp\\EventSourcing\\GiftCards";
                        //})
                        .UseDbEventStore<SampleDbContext, GiftCard, Guid>()
                        //.UseDynamoDBEventStore<GiftCard, Guid>(Configuration.GetSection("GiftCardEventStore"))
                        ;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            IEventStoreInitializer<GiftCard, Guid> eventStoreInitializer)
        {
            eventStoreInitializer.EnsureCreatedAsync().Wait();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc();
        }
    }
}
