using System.Collections;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace zpixFont
{
    public class zpixFont
    {
        private static IntPtr unmanagedPointer = IntPtr.Zero;
        private static long length;
        public static IntPtr getFont(out long len)
        {
            if (unmanagedPointer == IntPtr.Zero)
            {
                using (MemoryStream ms = new MemoryStream(fontzip.zpix))
                {
                    using (System.IO.Compression.ZipArchive za = new System.IO.Compression.ZipArchive(ms))
                    {
                        ZipArchiveEntry entry = za.GetEntry("zpix.ttf");
                        if (entry != null)
                        {
                            using (MemoryStream filedata = new MemoryStream())
                            {
                                using (Stream entryStream = entry.Open())
                                {
                                    entryStream.CopyTo(filedata);
                                }
                                var b = filedata.ToArray();
                                length = b.Length;
                                unmanagedPointer = Marshal.AllocHGlobal(b.Length);
                                Marshal.Copy(b, 0, unmanagedPointer, b.Length);
                            }
                        }
                    }
                }
            }
            len = length;
            return unmanagedPointer;
        }
    }
}
