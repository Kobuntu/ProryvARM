using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace Proryv.AskueARM2.Both.VisualCompHelpers
{
    public class CompressUtility
    {

        private static MemoryStream ReadAllBytes(GZipStream zip)
        {
            const int size = 4096;
            var buffer = new byte[size];
            var count = 0;
            var memory = new MemoryStream();

            do
            {
                count = zip.Read(buffer, 0, size);
                if (count > 0)
                {
                    memory.Write(buffer, 0, count);
                }
            } while (count > 0);
            return memory;
        }

        /// <summary>
        /// Расжимаем
        /// </summary>
        /// <param name="ms">MemoryStream который необходимо расжать</param>
        /// <returns>Расжатый MemoryStream</returns>
        public static MemoryStream DecompressGZip(MemoryStream ms)
        {
            if (ms == null) return null;

            var result = new MemoryStream();

            try
            {
                using (var zip = new GZipStream(ms, CompressionMode.Decompress, false))
                {
                    // Read all bytes in the zip stream and return them.
                    zip.CopyTo(result);
                }
                result.Position = 0;
            }
            catch
            {
                
            }

            return result;
        }

        /// <summary>
        /// Расжимаем
        /// </summary>
        /// <param name="ms">MemoryStream который необходимо расжать</param>
        /// <returns>Расжатый MemoryStream</returns>
        public static byte[] DecompressGZipToByte(MemoryStream ms)
        {
            if (ms == null) return null;

            using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress, false))
            {
                // Read all bytes in the zip stream and return them.
                return ReadAllBytes(zip).ToArray();
            }
        }

        /// <summary>
        /// Сжимаем
        /// </summary>
        /// <param name="ms">MemoryStream который необходимо расжать</param>
        /// <returns>Расжатый MemoryStream</returns>
        public static byte[] СompressGZipToByte(MemoryStream ms)
        {
            if (ms == null) return null;

            using (var zip = new GZipStream(ms, CompressionMode.Compress, false))
            {
                // Read all bytes in the zip stream and return them.
                return ReadAllBytes(zip).ToArray();
            }
        }

      

        /// <summary>
        /// Расжимаем
        /// </summary>
        /// <param name="ms">MemoryStream который необходимо расжать</param>
        /// <returns>Расжатый MemoryStream</returns>
        public static Stream DecompressGZip(Stream ms)
        {
            Stream result;
            if (!Environment.Is64BitProcess && ms.Length > 1024 * 50) // ??? Наверное должно хватить
            {
                //Если запущен под 32 битной версией IE тогда расжимаем в файл иначе OutOfMemoryException
                result = new TemporaryFileStream();
            }
            else
            {
                result = new MemoryStream();
            }



            using (var zip = new GZipStream(ms, CompressionMode.Decompress))
            {
                // Read all bytes in the zip stream and return them.
                zip.CopyTo(result);
            }
            result.Position = 0;
            return result;
        }


        class TemporaryFileStream : Stream, IDisposable
        {
            public TemporaryFileStream()
            {
                _file = new FileInfo(Path.GetTempFileName());
                _stream = new FileStream(
                    _file.FullName, FileMode.OpenOrCreate,
                    FileAccess.ReadWrite);
            }

            // Finalizer
            ~TemporaryFileStream()
            {
                System.Console.WriteLine("~TemporaryFileStream called");
                Close();
            }

            public FileStream Stream
            {
                get { return _stream; }
            }

            private readonly FileStream _stream;

            public FileInfo File
            {
                get { return _file; }
            }

            private readonly FileInfo _file;

            public void Close()
            {
                if (_stream != null)
                {
                    _stream.Close();
                }

                if (_file != null)
                {
                    _file.Delete();
                }

                // Turn off calling the finalizer
                //       System.GC.SuppressFinalize(this);
            }

            public void Dispose()
            {
                System.Console.WriteLine("IDisposable.Dispose called");
                Close();
            }

            public override void Flush()
            {
                _stream.Flush();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return _stream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                _stream.SetLength(value);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _stream.Read(buffer, offset, count);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _stream.Write(buffer, offset, count);
            }

            public override bool CanRead
            {
                get
                {
                    return _stream.CanRead;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return _stream.CanSeek;
                }
            }
            public override bool CanWrite
            {
                get
                {
                    return _stream.CanWrite;
                }
            }
            public override long Length
            {
                get
                {
                    return _stream.Length;
                }
            }
            public override long Position
            {
                get
                {
                    return _stream.Position;
                }
                set { _stream.Position = value; }
            }
        }



        public static MemoryStream CompressGZip(MemoryStream ms)
        {
            return CompressGZip(ms.ToArray());
        }

        public static MemoryStream CompressGZip(byte[] input)
        {
            var compressedms = new MemoryStream();
            using (var zip = new GZipStream(compressedms, CompressionMode.Compress, true))
            {
                // Write the input to the memory stream via the ZIP stream.
                zip.Write(input, 0, input.Length);
            }
            compressedms.Seek(0, SeekOrigin.Begin);
            return compressedms;
        }

        public static byte[] Zip(string str)
        {
            //var bytes = Convert.FromBase64String(str);
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        public static string Unzip(byte[] bytes, bool useBase64 = false)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return useBase64 ? Convert.ToBase64String(mso.ToArray())
                    : Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }
    }
}
