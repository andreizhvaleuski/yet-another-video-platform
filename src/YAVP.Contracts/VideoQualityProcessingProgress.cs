using MessagePack;

namespace YAVP.Contracts
{
    [MessagePackObject]
    public sealed class VideoQualityKey
    {
        [SerializationConstructor]
        public VideoQualityKey(Guid id, int quality)
        {
            Id = id;
            Quality = quality;
        }

        [Key(0)]
        public Guid Id { get; }

        [Key(1)]
        public int Quality { get; }
    }

    [MessagePackObject]
    public sealed class VideoQualityProcessingProgress
    {
        [SerializationConstructor]
        public VideoQualityProcessingProgress(bool isProcessed, double speed, TimeSpan outTime)
        {
            IsProcessed = isProcessed;
            Speed = speed;
            OutTime = outTime;
        }

        [Key(1)]
        public bool IsProcessed { get; }

        [Key(2)]
        public double Speed { get; }

        [Key(3)]
        public TimeSpan OutTime { get; }
    }
}
