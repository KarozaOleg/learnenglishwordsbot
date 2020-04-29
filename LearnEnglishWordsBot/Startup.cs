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
using System.Threading.Tasks;

namespace LearnEnglishWordsBot
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.Configure<DatabaseSettings>(Configuration.GetSection("ConnectionStrings"));
            services.Configure<JobTriggersSettigns>(Configuration.GetSection(nameof(JobTriggersSettigns)));
            services.Configure<BotSettings>(Configuration.GetSection(nameof(BotSettings)));

            services.AddSingleton<QuartzStartup>();
            services.AddTransient<CreateTasksToLearnJob>();

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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();                
                app.UseRouter(new RouteBuilder(app, new RouteHandler(CreateTasksToLearn)).MapRoute("createTasksToLearn", "createTasksToLearn").Build());
            }
            else
            {
                app.UseHsts();
            }

            var quartz = app.ApplicationServices.GetService<QuartzStartup>();

            lifetime.ApplicationStarted.Register(async () => await quartz.SetStart());
            lifetime.ApplicationStopping.Register(quartz.SetStop);

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        async Task CreateTasksToLearn(HttpContext context)
        {
            var job = context.RequestServices.GetService<CreateTasksToLearnJob>();
            await job.Execute(null);
            await context.Response.WriteAsync("CreateTasksToLearnJob job is done!");
        }
    }
}
