using AdvUtils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Seq2SeqWebAPI;
using SeqClassificationWebAPI;
using SeqSimilarityWebAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SeqWebAPIs
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            if (String.IsNullOrEmpty(Configuration["Seq2Seq:ModelFilePath"]) == false)
            {
                Logger.WriteLine($"Loading Seq2Seq model '{Configuration["Seq2Seq:ModelFilePath"]}'");

                var modelFilePath = Configuration["Seq2Seq:ModelFilePath"];
                int maxTestSrcSentLength = int.Parse(Configuration["Seq2Seq:MaxSrcTokenSize"]);
                int maxTestTgtSentLength = int.Parse(Configuration["Seq2Seq:MaxTgtTokenSize"]);
                var processorType = Configuration["Seq2Seq:ProcessorType"];

                Seq2SeqInstance.Initialization(modelFilePath, maxTestSrcSentLength, maxTestTgtSentLength, processorType);
            }

            if (String.IsNullOrEmpty(Configuration["SeqClassification:ModelFilePath"]) == false)
            {
                Logger.WriteLine($"Loading SeqClassification model '{Configuration["SeqClassification:ModelFilePath"]}'");

                var modelFilePath = Configuration["SeqClassification:ModelFilePath"];
                int maxTestSentLength = int.Parse(Configuration["SeqClassification:MaxTokenSize"]);
                var processorType = Configuration["SeqClassification:ProcessorType"];

                SeqClassificationInstance.Initialization(modelFilePath, maxTestSentLength, processorType);
            }

            if (String.IsNullOrEmpty(Configuration["SeqSimilarity:ModelFilePath"]) == false)
            {
                Logger.WriteLine($"Loading SeqSimilarity model '{Configuration["SeqSimilarity:ModelFilePath"]}'");

                var modelFilePath = Configuration["SeqSimilarity:ModelFilePath"];
                int maxTestSentLength = int.Parse(Configuration["SeqSimilarity:MaxTokenSize"]);
                var processorType = Configuration["SeqSimilarity:ProcessorType"];

                SeqSimilarityInstance.Initialization(modelFilePath, maxTestSentLength, processorType);
            }
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SeqWebAPIs", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SeqWebAPIs v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
