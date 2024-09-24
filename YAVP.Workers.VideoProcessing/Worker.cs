using Confluent.Kafka;
using YAVP.Contracts;

namespace YAVP.Workers.VideoProcessing
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IVideoProcessor _videoProcessor;

        public Worker(ILogger<Worker> logger, IVideoProcessor videoProcessor)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(videoProcessor, nameof(videoProcessor));

            _logger = logger;
            _videoProcessor = videoProcessor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:33101,localhost:33102,localhost:33103",
                GroupId = "360p-Video-Processors",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                HeartbeatIntervalMs = 1000,
                MaxPollIntervalMs = 10_000_00
            };

            using (var consumer = new ConsumerBuilder<Null, VideoUploaded>(config)
                .SetValueDeserializer(new KafkaMessagePackDeserializer<VideoUploaded>())
                .SetLogHandler((consumer, log) =>
                {
                    _logger.Log(
                        logLevel: (LogLevel)log.LevelAs(LogLevelType.MicrosoftExtensionsLogging),
                        eventId: 0,
                        exception: null,
                        message: log.Message,
                        log.Name,
                        log.Facility
                    );
                })
                .Build())
            {
                consumer.Subscribe("360p-Videos");

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(stoppingToken);

                        var videoUploaded = consumeResult.Message.Value;
                        _logger.LogInformation(
                            "Starting to process Video ('{VideoId}') located at '{FileLocation}'.",
                            videoUploaded.VideoId,
                            videoUploaded.FileLocation);

                        await _videoProcessor.ProcessAsync(
                            videoUploaded.VideoId,
                            videoUploaded.FileLocation);

                        consumer.Commit();
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(
                            exception,
                            @"Failed to process a message. Exiting the worker");

                        break;
                    }
                }

                consumer.Close();
            }
        }
    }
}
