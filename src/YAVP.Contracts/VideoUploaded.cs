namespace YAVP.Contracts
{
    public sealed class VideoUploaded
    {
        public Guid VideoId { get; set; }

        public string FileLocation { get; set; }
    }
}
