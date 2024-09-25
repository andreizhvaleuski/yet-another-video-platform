using System.Diagnostics;

namespace YAVP.Workers.VideoProcessing
{
    public partial class FfmpegVideoProcessor : IVideoProcessor
    {
        private readonly ILogger _logger;
        private readonly IProgressReporter _progressReporter;

        public FfmpegVideoProcessor(ILogger<FfmpegVideoProcessor> logger, IProgressReporter progressReporter)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(progressReporter, nameof(progressReporter));

            _logger = logger;
            _progressReporter = progressReporter;
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
                .WithFfmpegProgress(statsPeriod: TimeSpan.FromSeconds(5))
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
                .AddArgument("dash.mpd");

            var workingDirectory = Path.Combine(@"D:\YAVP\Videos", "360p", videoId.ToString());
            Directory.CreateDirectory(workingDirectory);
            process.StartInfo.WorkingDirectory = workingDirectory;

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
                    _progressReporter.ReportProgress(
                        videoId,
                        360,
                        new VideoQualityProcissongProgress(
                            IsProcesseed: value == "end",
                            ProcessingSpeed: double.Parse(dict["speed"][..^1]),
                            OutputTime: TimeSpan.FromMilliseconds(long.Parse(dict["out_time_ms"]))));

                    dict.Clear();
                }
            }
        }

        private void ProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data is null)
            {
                return;
            }

            ErrorOccuredDuringExecution(e.Data);
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
}
