using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BarsParser
{
    public class MarkerInfo
    {
        public uint Id
        {
            get;
            set;
        }

        public uint Metadata
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public uint StartPosition
        {
            get;
            set;
        }

        public uint Length
        {
            get;
            set;
        }

        public bool IsRegion
        {
            get
            {
                return Length != 0;
            }
        }

        public MarkerInfo(BinaryStream binaryStream, uint stringTableOffset)
        {
            // Read the fields
            Id = binaryStream.ReadUInt32(); // does idx << 4 and reads that offset, which ends up being this
            Metadata = Id;
            uint stringOffset = binaryStream.ReadUInt32();
            StartPosition = binaryStream.ReadUInt32();
            Length = binaryStream.ReadUInt32();

            // Seek to the string and read it
            using (binaryStream.TemporarySeek(stringTableOffset + stringOffset, SeekOrigin.Begin))
            {
                Name = binaryStream.ReadString();
            }
        }

    }
}
