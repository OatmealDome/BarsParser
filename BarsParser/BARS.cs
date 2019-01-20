using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BarsParser
{
    public class BARS
    {
        private List<byte[]> streamFiles = new List<byte[]>();
        private List<byte[]> metadataFiles = new List<byte[]>();

        public int Count
        {
            get
            {
                return streamFiles.Count;
            }
        }
        
        public BARS()
        {

        }

        public BARS(Stream stream)
        {
            using (BinaryStream binaryStream = new BinaryStream(stream))
            {
                // Strings are zero-terminated
                binaryStream.StringCoding = StringCoding.ZeroTerminated;

                // Check magic numbers
                if (binaryStream.ReadUInt32() != 0x53524142) // "BARS"
                {
                    throw new Exception("Not a BARS file");
                }

                // Read file size
                uint fileSize = binaryStream.ReadUInt32();

                // Check endianness
                if (binaryStream.ReadUInt16() == 0xFFFE)
                {
                    // Set the stream to little endian
                    binaryStream.ByteConverter = ByteConverter.Little;
                }

                // Read version(?)
                // aal::AudioResource stops reading the file if this isn't 0x101
                if (binaryStream.ReadUInt16() != 0x101)
                {
                    throw new Exception("Offset 0xA in a BARS file must be 0x101");
                }

                // Read number of entries
                uint entryCount = binaryStream.ReadUInt32();

                // Read CRC32 hashes
                uint[] hashes = binaryStream.ReadUInt32s((int)entryCount);

                // Loop over every entry
                for (int i = 0; i < entryCount; i++)
                {
                    // Seek to the metadata
                    using (binaryStream.TemporarySeek(binaryStream.ReadUInt32(), SeekOrigin.Begin))
                    {
                        // Verify the magic numbers
                        if (binaryStream.ReadString(4) != "AMTA")
                        {
                            throw new Exception("Something went wrong, AMTA not found where it's supposed to be");
                        }

                        // Set the stream to big endian
                        binaryStream.ByteConverter = ByteConverter.Big;

                        // Check endianness
                        if (binaryStream.ReadUInt16() == 0xFFFE)
                        {
                            // Set the stream to little endian
                            binaryStream.ByteConverter = ByteConverter.Little;
                        }

                        // Skip unknowns
                        binaryStream.Seek(2);

                        // Load the size
                        uint metaSize = binaryStream.ReadUInt32();

                        // Seek backwards past the size and magic numbers
                        binaryStream.Seek(-12);

                        // Read the file
                        byte[] rawMetadata = binaryStream.ReadBytes((int)metaSize);

                        // Load the file
                        metadataFiles.Add(rawMetadata);

                        using (MemoryStream memoryStream = new MemoryStream(rawMetadata))
                        {
                            AudioMetadata metadata = new AudioMetadata(memoryStream);
                            Console.WriteLine(metadata.SampleRate);
                        }
                    }

                    // Seek to the stream
                    using (binaryStream.TemporarySeek(binaryStream.ReadUInt32(), SeekOrigin.Begin))
                    {
                        // Verify the magic numbers
                        string streamMagic = binaryStream.ReadString(4);
                        if (streamMagic != "FWAV" && streamMagic != "FSTP")
                        {
                            throw new Exception("Something went wrong, stream not found where it's supposed to be");
                        }

                        // Set the stream to big endian
                        binaryStream.ByteConverter = ByteConverter.Big;

                        // Check endianness
                        if (binaryStream.ReadUInt16() == 0xFFFE)
                        {
                            // Set the stream to little endian
                            binaryStream.ByteConverter = ByteConverter.Little;
                        }

                        // Skip unknowns
                        binaryStream.Seek(6);

                        // Load the size
                        uint streamSize = binaryStream.ReadUInt32();

                        // Seek backwards past the size and magic numbers
                        binaryStream.Seek(-16);

                        // Load the file
                        streamFiles.Add(binaryStream.ReadBytes((int)streamSize));
                    }
                }
            }
        }

        public byte[] GetStream(int index)
        {
            return streamFiles[index];
        }

        public byte[] GetMetadata(int index)
        {
            return metadataFiles[index];
        }

        public void Add(byte[] rawStream, byte[] rawMetadata)
        {
            streamFiles.Add(rawStream);
            metadataFiles.Add(rawMetadata);
        }

        public void Remove(int index)
        {
            streamFiles.RemoveAt(index);
            metadataFiles.RemoveAt(index);
        }

    }
}
