using LearnEnglishWordsBot.Extensions;
using LearnEnglishWordsBot.Interfaces;
using LearnEnglishWordsBot.Jobs;
using LearnEnglishWordsBot.Repositories;
using LearnEnglishWordsBot.Services;
using LearnEnglishWordsBot.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace LearnEnglishWordsBot
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private ILogger Logger { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            var nlogLoggerProvider = new NLogLoggerProvider();
            Logger = nlogLoggerProvider.CreateLogger(typeof(Startup).FullName);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0).AddNewtonsoftJson();

            services.Configure<DatabaseSettings>(Configuration.GetSection("ConnectionStrings"));
            services.Configure<JobTriggersSettigns>(Configuration.GetSection(nameof(JobTriggersSettigns)));
            services.Configure<BotSettings>(Configuration.GetSection(nameof(BotSettings)));
            
            services.AddQuartz(q => 
            {
                q.UseMicrosoftDependencyInjectionScopedJobFactory();
                q.AddJobAndTrigger<CreateTasksToLearnJob>(Configuration);
            });
            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            services.AddSingleton<IUsersRepository, UsersRepository>();
            services.AddSingleton<ILearnService, LearnService>();
            services.AddSingleton<INotifyService, TelegramNotifyService>();
            services.AddTransient<IMessagesRepository, MessagesRepository>();
            services.AddTransient<IAnswersRepository, AnswersRepository>();
            services.AddTransient<IWordsRepository, WordsRepository>();
            services.AddTransient<ILearnTaskService, LearnTaskService>();
            services.AddTransient<ILearnSetRepository, LearnSetRepository>();
            services.AddTransient<ILearnSetService, LearnSetService>();
            services.AddTransient<ILearnTaskRepository, LearnTaskRepository>();
            services.AddTransient<CreateTasksToLearnJob>();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConfiguration(Configuration.GetSection("Logging"));
                loggingBuilder.AddNLog(Configuration);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseRouter(new RouteBuilder(app, new RouteHandler(CreateTasksToLearn)).MapRoute("createTasksToLearn", "createTasksToLearn").Build());
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}");
            });
        }

        async Task CreateTasksToLearn(HttpContext context)
        {
            try
            {
                var job = context.RequestServices.GetService<CreateTasksToLearnJob>();
                await job.Execute(null);
                await context.Response.WriteAsync("CreateTasksToLearnJob job is done!");
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "error happend during launching CreateTasksToLearnJob job");
                throw;
            }
        }
    }
}
