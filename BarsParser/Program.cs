using System;
using System.IO;

namespace BarsParser
{
    class Program
    {
        static void Main(string[] args)
        {
            using (FileStream fileStream = new FileStream(args[0], FileMode.Open))
            {
                BARS bars = new BARS(fileStream);

                string outputPath = Path.Combine(Path.GetFullPath(Path.GetDirectoryName(args[0])), Path.GetFileNameWithoutExtension(args[0]));
                Directory.CreateDirectory(outputPath);

                for (int i = 0; i < bars.Count; i++)
                {
                    byte[] streamData = bars.GetStream(i);
                    string streamPath = Path.Combine(outputPath, i.ToString() + (streamData[1] == 'W' ? ".bfwav" : ".bfstp"));
                    File.WriteAllBytes(streamPath, streamData);

                    byte[] metadataData = bars.GetMetadata(i);
                    string metadataPath = Path.Combine(outputPath, i.ToString() + ".bameta");
                    File.WriteAllBytes(metadataPath, metadataData);
                }
            }
        }
    }
}
