using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BarsParser
{
    class ExtEntry
    {
        public string Name
        {
            get;
            set;
        }

        public uint Unknown
        {
            get;
            set;
        }

        public ExtEntry(BinaryStream binaryStream, uint strgOffset)
        {
            // Read fields
            uint nameOffset = binaryStream.ReadUInt32();
            Unknown = binaryStream.ReadUInt32();

            // Read the name of this entry
            using (binaryStream.TemporarySeek(strgOffset + nameOffset, SeekOrigin.Begin))
            {
                Name = binaryStream.ReadString();
            }
        }

    }
}
