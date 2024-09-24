using MessagePack;

namespace YAVP.Contracts
{
    [MessagePackObject]
    public sealed class VideoUploaded
    {
        [SerializationConstructor]
        public VideoUploaded(Guid videoId, string fileLocation)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileLocation, nameof(fileLocation));

            VideoId = videoId;
            FileLocation = fileLocation;
        }

        [Key(0)]
        public Guid VideoId { get; }

        [Key(1)]
        public string FileLocation { get; }
    }
}
