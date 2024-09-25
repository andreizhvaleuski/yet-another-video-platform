namespace YAVP.Workers.VideoProcessing
{
    public interface IVideoProcessor
    {
        public Task ProcessAsync(
            Guid videoId,
            string fileLocation,
            CancellationToken stoppingToken = default);
    }
}
