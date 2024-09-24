using Serilog;

namespace YAVP.Workers.VideoProcessing
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<Worker>();
            builder.Services.AddSingleton<IVideoProcessor, FfmpegVideoProcessor>();
            builder.Services.AddSerilog(configuration =>
                configuration.ReadFrom.Configuration(builder.Configuration));

            var host = builder.Build();
            host.Run();

            return 0;
        }
    }
}