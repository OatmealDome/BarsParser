using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BarsParser
{
    class AudioMetadata
    {
        public string Name
        {
            get;
            set;
        }

        public uint Unknown1
        {
            get;
            set;
        }

        public byte Unknown2
        {
            get;
            set;
        }

        public byte ChannelCount
        {
            get;
            set;
        }

        public byte TrackCount
        {
            get;
            set;
        }

        public byte Unknown3
        {
            get;
            set;
        }

        public uint Unknown4
        {
            get;
            set;
        }

        public uint SampleRate
        {
            get;
            set;
        }

        public uint LoopStartSample
        {
            get;
            set;
        }

        public uint LoopEndSample
        {
            get;
            set;
        }

        public float MinimumAmplitude
        {
            get;
            set;
        }

        public List<TrackInfo> TrackInfos = new List<TrackInfo>();

        public float PeakAmplitude
        {
            get;
            set;
        }

        public List<MarkerInfo> Markers = new List<MarkerInfo>();

        public List<ExtEntry> ExtEntries = new List<ExtEntry>();

        public AudioMetadata(Stream stream)
        {
            using (BinaryStream binaryStream = new BinaryStream(stream))
            {
                // Strings are zero-terminated
                binaryStream.StringCoding = StringCoding.ZeroTerminated;

                Read(binaryStream);
            }
        }

        public AudioMetadata(BinaryStream binaryStream)
        {
            Read(binaryStream);
        }

        private void Read(BinaryStream binaryStream)
        {
            // Check magic numbers
            if (binaryStream.ReadString(4) != "AMTA")
            {
                throw new Exception("Not an AudioMetadata file");
            }

            // Check endianness
            if (binaryStream.ReadUInt16() == 0xFFFE)
            {
                // Set the stream to little endian
                binaryStream.ByteConverter = ByteConverter.Little;
            }

            // Read version
            uint version = binaryStream.ReadUInt16();
            if (version != 0x400)
            {
                throw new Exception("AudioMetadata versions other than 0x400 are not supported");
            }

            // Read fields and offsets
            uint size = binaryStream.ReadUInt32();
            uint dataOffset = binaryStream.ReadUInt32();
            uint markOffset = binaryStream.ReadUInt32();
            uint extOffset = binaryStream.ReadUInt32();
            uint strgOffset = binaryStream.ReadUInt32() + 8; // skip section size and magic
            
            // Read data section
            using (binaryStream.TemporarySeek(dataOffset, SeekOrigin.Begin))
            {
                // Check header
                if (binaryStream.ReadString(4) != "DATA")
                {
                    throw new Exception("DATA section not found, something went wrong");
                }

                // Begin reading fields
                uint dataSize = binaryStream.ReadUInt32();
                uint nameOffset = binaryStream.ReadUInt32();

                // Read name
                using (binaryStream.TemporarySeek(strgOffset + nameOffset, SeekOrigin.Begin))
                {
                    Name = binaryStream.ReadString();
                }

                // Continue reading fields
                Unknown1 = binaryStream.ReadUInt32();
                Unknown2 = binaryStream.Read1Byte();
                ChannelCount = binaryStream.Read1Byte();
                TrackCount = binaryStream.Read1Byte();
                Unknown3 = binaryStream.Read1Byte();
                Unknown4 = binaryStream.ReadUInt32();
                SampleRate = binaryStream.ReadUInt32();
                LoopStartSample = binaryStream.ReadUInt32();
                LoopEndSample = binaryStream.ReadUInt32();
                MinimumAmplitude = binaryStream.ReadSingle();

                // Read all 8 TrackInfos
                for (int i = 0; i < 8; i++)
                {
                    TrackInfos.Add(new TrackInfo(binaryStream));
                }

                // Read peak amplitude
                PeakAmplitude = binaryStream.ReadSingle();
            }

            // Read MarkerInfos
            using (binaryStream.TemporarySeek(markOffset, SeekOrigin.Begin))
            {
                // Check header
                if (binaryStream.ReadString(4) != "MARK")
                {
                    throw new Exception("MARK section not found, something went wrong");
                }

                // Read fields
                uint markSize = binaryStream.ReadUInt32();
                uint markCount = binaryStream.ReadUInt32();

                // Read MarkerInfos
                for (int i = 0; i < markCount; i++)
                {
                    Markers.Add(new MarkerInfo(binaryStream, strgOffset));
                }
            }

            // Read EXT_ section
            using (binaryStream.TemporarySeek(extOffset, SeekOrigin.Begin))
            {
                // Check header
                if (binaryStream.ReadString(4) != "EXT_")
                {
                    throw new Exception("EXT_ section not found, something went wrong");
                }

                // Read fields
                uint extSize = binaryStream.ReadUInt32();
                uint extCount = binaryStream.ReadUInt32();

                // Read EXT_ entries
                for (int i = 0; i < extCount; i++)
                {
                    ExtEntries.Add(new ExtEntry(binaryStream, strgOffset));
                }
            }
        }

        // NOTES

        // DATA section
        // ------------
        // "DATA" magic numbers
        // section size
        // string table offset for name
        // 4 bytes unk
        // 1 byte unk
        // number of channels
        // total tracks in stream
        // loop OK flag?? (maybe a bitfield? set to 0x7 in looping versus bgm, set to 0x2 in versus 1 min remaining)
        // 4 byte unk
        // sample rate (usually 44100Hz)
        // loop start sample
        // loop end sample
        // float - has something to do with volume? perhaps min volume
        // track info x 8
        // peak amplitude value as float

        // TrackInfo
        // ---------
        // 4 byte unk - # of channels?
        // float unk - track volume?

    }
}
