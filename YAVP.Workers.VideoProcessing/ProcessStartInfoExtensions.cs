using System.Diagnostics;

namespace YAVP.Workers.VideoProcessing
{
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
