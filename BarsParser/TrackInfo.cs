using Syroot.BinaryData;

namespace BarsParser
{
    public class TrackInfo
    {
        public uint ChannelCount
        {
            get;
            set;
        }

        public float Volume
        {
            get;
            set;
        }

        public TrackInfo(BinaryStream binaryStream)
        {
            // Read fields
            ChannelCount = binaryStream.ReadUInt32();
            Volume = binaryStream.ReadSingle();
        }

    }
}
