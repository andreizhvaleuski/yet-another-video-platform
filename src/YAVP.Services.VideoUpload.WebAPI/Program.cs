using Confluent.Kafka;
using tusdotnet;
using YAVP.Contracts;

namespace YAVP.Services.VideoUpload.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //builder.services.addcors(options =>
            //{
            //    options.adddefaultpolicy(policy => policy
            //        .allowanyheader()
            //        .allowanymethod()
            //        .allowanyorigin()
            //        .withexposedheaders(corshelper.getexposedheaders()));
            //});

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            //app.UseCors();

            app.UseAuthorization();

            app.MapTus("/video/{videoId:required:guid}/upload", async httpContext =>
            {
                var videoId = Guid.Parse(httpContext.GetRouteValue("videoId") as string);

                Directory.CreateDirectory(@$"D:\Video\Source\{videoId}");

                return new()
                {
                    // Where to store data?
                    Store = new tusdotnet.Stores.TusDiskStore(@$"D:\Video\Source\{videoId}"),
                    Events = new()
                    {
                        // What to do when file is completely uploaded?
                        OnFileCompleteAsync = async eventContext =>
                        {
                            var config = new ProducerConfig
                            {
                                BootstrapServers = "localhost:33101,localhost:33102,localhost:33103",
                                EnableIdempotence = true,
                            };

                            using (var producer = new ProducerBuilder<Null, VideoUploaded>(config)
                                .SetValueSerializer(new KafkaMessagePackSerializer<VideoUploaded>())
                                .Build())
                            {
                                await producer.ProduceAsync(
                                    "360p-Videos",
                                    new Message<Null, VideoUploaded>()
                                    {
                                        Value = new VideoUploaded(videoId, @$"D:\Video\Source\{videoId}\{eventContext.FileId}")
                                    }, eventContext.CancellationToken);
                            }
                        }
                    }
                };
            })
            .WithOpenApi();

            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}
