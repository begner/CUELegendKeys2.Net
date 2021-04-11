using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CueLegendKey2
{
    public class FileCache
    {
        private string subPath = "";
        public void SetSubPath(string subPath)
        {
            this.subPath = subPath;
        }

        private string fileName = "";
        public void SetFileName(string fileName)
        {
            this.fileName = fileName;
        }

        private string GetCachePath()
        {
            return $"./.cache/{subPath}";
        }
        private string GetFilePath()
        {
            return $"{this.GetCachePath()}/{fileName}";
        }

        public bool Exists()
        {
            return File.Exists(this.GetFilePath());
        }

        public void Write(Stream s)
        {
            Directory.CreateDirectory(this.GetCachePath());
            using (var fileStream = File.Create(this.GetFilePath()))
            {
                // s.Seek(0, SeekOrigin.Begin);
                s.CopyTo(fileStream);
            }
        }

        public Stream Read()
        {
            Byte[] byteArray = File.ReadAllBytes(this.GetFilePath());
            Stream stream = new MemoryStream(byteArray);
            return stream;
        }
    }
    
}
