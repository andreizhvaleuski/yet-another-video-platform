using Confluent.Kafka;
using Serilog;
using YAVP.Contracts.Kafka;

namespace YAVP.Workers.VideoProcessing
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<Worker>();
            builder.Services.AddSingleton<IVideoProcessor, FfmpegVideoProcessor>();
            builder.Services.AddSingleton<IProgressReporter, KafkaProgressReporter>();
            builder.Services.AddSingleton(typeof(IDeserializer<>), typeof(KafkaMessagePackDeserializer<>));
            builder.Services.AddSingleton(typeof(ISerializer<>), typeof(KafkaMessagePackSerializer<>));
            builder.Services.AddSerilog(configuration =>
                configuration.ReadFrom.Configuration(builder.Configuration));

            var host = builder.Build();
            host.Run();

            return 0;
        }
    }
}
