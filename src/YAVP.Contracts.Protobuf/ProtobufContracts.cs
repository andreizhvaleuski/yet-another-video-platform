using ProtoBuf.Meta;

namespace YAVP.Contracts.Protobuf
{
    public class ProtobufContracts : IContracts
    {
        public void Configure()
        {
            RuntimeTypeModel.Default.Add<VideoUploaded>()
                .Add(1, nameof(VideoUploaded.VideoId))
                .Add(2, nameof(VideoUploaded.FileLocation));
        }
    }
}
