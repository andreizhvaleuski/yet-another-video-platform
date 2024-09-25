using Confluent.Kafka;
using YAVP.Contracts;

namespace YAVP.Workers.VideoProcessing
{
    public sealed class KafkaProgressReporter : IProgressReporter
    {
        private readonly Lazy<IProducer<VideoQualityKey, VideoQualityProcessingProgress>> _producer;

        public KafkaProgressReporter(
            ISerializer<VideoQualityKey> videoQualityKeySerializer,
            ISerializer<VideoQualityProcessingProgress> videoQualityProcessingProgressSerializer,
            ILogger<KafkaProgressReporter> logger)
        {
            ArgumentNullException.ThrowIfNull(videoQualityKeySerializer, nameof(videoQualityKeySerializer));
            ArgumentNullException.ThrowIfNull(videoQualityProcessingProgressSerializer, nameof(videoQualityProcessingProgressSerializer));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));

            _producer = new Lazy<IProducer<VideoQualityKey, VideoQualityProcessingProgress>>(
                valueFactory: () =>
                {
                    var config = new ProducerConfig
                    {
                        BootstrapServers = "localhost:33101,localhost:33102,localhost:33103",
                        EnableIdempotence = true,
                    };

                    var producerBuilder = new ProducerBuilder<VideoQualityKey, VideoQualityProcessingProgress>(config)
                        .SetKeySerializer(videoQualityKeySerializer)
                        .SetValueSerializer(videoQualityProcessingProgressSerializer)
                        .SetLogHandler((producer, logMessage) =>
                        {
                            logger.Log(
                                logLevel: (LogLevel)logMessage.LevelAs(LogLevelType.MicrosoftExtensionsLogging),
                                eventId: 0,
                                exception: null,
                                message: logMessage.Message,
                                logMessage.Name,
                                logMessage.Facility
                            );
                        })
                        .SetErrorHandler((producer, error) =>
                        {
                            logger.Log(
                                logLevel: LogLevel.Error,
                                eventId: 0,
                                exception: null,
                                message: error.Reason
                            );
                        });

                    return producerBuilder.Build();
                },
                isThreadSafe: true);
        }

        public void ReportProgress(Guid videoId, int videoQuality, VideoQualityProcissongProgress progress)
        {
            _producer.Value.Produce(
                "video.quality.processing.progress",
                new Message<VideoQualityKey, VideoQualityProcessingProgress>
                {
                    Key = new VideoQualityKey(
                        videoId,
                        videoQuality),
                    Value = new VideoQualityProcessingProgress(
                        progress.IsProcesseed,
                        progress.ProcessingSpeed,
                        progress.OutputTime)
                });
        }
    }

    public interface IProgressReporter
    {
        public void ReportProgress(
            Guid videoId,
            int videoQuality,
            VideoQualityProcissongProgress progress);
    }

    public record struct VideoQualityProcissongProgress(
        bool IsProcesseed,
        double ProcessingSpeed,
        TimeSpan OutputTime);
}
