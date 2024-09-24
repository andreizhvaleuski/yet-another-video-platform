using System.Diagnostics;

namespace YAVP.Workers.VideoProcessing
{
    public interface IVideoProcessor
    {
        public Task ProcessAsync(
            Guid videoId,
            string fileLocation,
            CancellationToken stoppingToken = default);
    }

    public partial class FfmpegVideoProcessor : IVideoProcessor
    {
        private readonly ILogger _logger;

        public FfmpegVideoProcessor(ILogger<FfmpegVideoProcessor> logger)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));

            _logger = logger;
        }

        public async Task ProcessAsync(
            Guid videoId,
            string fileLocation,
            CancellationToken stoppingToken = default)
        {
            using var process = new Process();

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            process.StartInfo.FileName = "ffmpeg";

            process.StartInfo
                .WithHideBanner()
                .DoNotOverwriteOutputFiles()
                .WithFfmpegProgress()
                .WithLogLevel("repeat", "level", "error")
                .WithInput(fileLocation)
                .AddArgument("-r").AddArgument("25")
                .AddArgument("-c:v").AddArgument("libx264")
                .AddArgument("-pix_fmt").AddArgument("yuv420p")
                .AddArgument("-preset").AddArgument("fast")
                .AddArgument("-profile:v").AddArgument("main")
                .AddArgument("-keyint_min").AddArgument("250")
                .AddArgument("-g").AddArgument("250")
                .AddArgument("-sc_threshold").AddArgument("0")
                .AddArgument("-c:a").AddArgument("aac")
                .AddArgument("-b:a").AddArgument("128k")
                .AddArgument("-ac").AddArgument("2")
                .AddArgument("-ar").AddArgument("48000")
                .AddArgument("-map").AddArgument("v:0")
                .AddArgument("-filter:v:0").AddArgument("scale=-2:360")
                .AddArgument("-b:v:0").AddArgument("800k")
                .AddArgument("-maxrate:0").AddArgument("856k")
                .AddArgument("-bufsize:0").AddArgument("1200k")
                .AddArgument("-map").AddArgument("0:a")
                .AddArgument("-init_seg_name").AddArgument("init-$RepresentationID$.$ext$")
                .AddArgument("-media_seg_name").AddArgument("chunk-$RepresentationID$-$Number%05d$.$ext$")
                .AddArgument("-dash_segment_type").AddArgument("mp4")
                .AddArgument("-use_template").AddArgument("1")
                .AddArgument("-use_timeline").AddArgument("0")
                .AddArgument("-seg_duration").AddArgument("10")
                .AddArgument("-adaptation_sets").AddArgument("id=0,streams=v id=1,streams=a")
                .AddArgument("-f").AddArgument("dash")
                .AddArgument(Path.Combine(@"D:\YAVP\Videos", "360p", videoId.ToString(), "dash.mpd"));

            Directory.CreateDirectory(Path.Combine(@"D:\YAVP\Videos", "360p", videoId.ToString()));

            var dict = new Dictionary<string, string?>();

            process.OutputDataReceived += ProcessOutputDataReceived;
            process.ErrorDataReceived += ProcessErrorDataReceived;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();


            await process.WaitForExitAsync(stoppingToken);

            void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                var d = e.Data?.Split('=', StringSplitOptions.TrimEntries);
                var key = d?[0];
                var value = d?[1];

                if (key is null)
                {
                    return;
                }

                dict.Add(key, value);

                if (key == "progress")
                {
                    Console.WriteLine(string.Join(";", dict.Select(x => x.Key + "=" + x.Value)));
                    dict.Clear();
                }
            }
        }

        private void ProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data is null)
            {
                ErrorOccuredDuringExecution();
            }
            else
            {
                ErrorOccuredDuringExecution(e.Data);
            }
        }


        [LoggerMessage(
            Level = LogLevel.Error,
            Message = "Error occured during process execution: {Message}")]
        public partial void ErrorOccuredDuringExecution(string message);

        [LoggerMessage(
            Level = LogLevel.Error,
            Message = "Error occured during process execution")]
        public partial void ErrorOccuredDuringExecution();
    }

    public static class ProcessStartInfoExtensions
    {
        public static ProcessStartInfo AddArgument(
            this ProcessStartInfo processStartInfo,
            string argument)
        {
            processStartInfo.ArgumentList.Add(argument);

            return processStartInfo;
        }

        public static ProcessStartInfo WithFfmpegProgress(
            this ProcessStartInfo processStartInfo,
            string url = "-",
            TimeSpan? statsPeriod = null)
        {
            processStartInfo.ArgumentList.Add("-progress");
            processStartInfo.ArgumentList.Add(url);

            if (statsPeriod.HasValue)
            {
                processStartInfo.ArgumentList.Add("-stats_period");
                processStartInfo.ArgumentList.Add(statsPeriod.Value.TotalSeconds.ToString());
            }

            return processStartInfo;
        }

        public static ProcessStartInfo WithLogLevel(
            this ProcessStartInfo processStartInfo,
            params string[] flags)
        {
            processStartInfo.ArgumentList.Add("-loglevel");
            processStartInfo.ArgumentList.Add(string.Join('+', flags));

            return processStartInfo;
        }

        public static ProcessStartInfo WithHideBanner(this ProcessStartInfo processStartInfo)
        {
            processStartInfo.ArgumentList.Add("-hide_banner");

            return processStartInfo;
        }

        public static ProcessStartInfo WithInput(
            this ProcessStartInfo processStartInfo,
            string url)
        {
            processStartInfo.ArgumentList.Add("-i");
            processStartInfo.ArgumentList.Add(url);

            return processStartInfo;
        }

        public static ProcessStartInfo DoNotOverwriteOutputFiles(this ProcessStartInfo processStartInfo)
        {
            processStartInfo.ArgumentList.Add("-n");

            return processStartInfo;
        }
    }
}
