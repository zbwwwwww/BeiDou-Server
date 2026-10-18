// Minimal-build stubs for third-party types used only in code paths
// that the skill-img editor never invokes (PNG decode, MP3 import, etc.)
using System;

namespace MapleLib
{
    // Stub that reports "no native lib loaded" so WzPngProperty falls back
    // to its built-in managed DXT decoder. Never touches libapng.dll.
    public static class SquishPNGWrapper
    {
        public enum FlagsEnum
        {
            kDxt1 = 1,
            kDxt3 = 2,
            kDxt5 = 4
        }

        public static bool CheckAndLoadLibrary()
        {
            return false;
        }

        public static void DecompressImage(byte[] dest, int width, int height, byte[] src, int flags)
        {
            throw new NotSupportedException("native libapng not available in minimal build");
        }

        public static void CompressImage(byte[] src, int width, int height, byte[] dest, int flags)
        {
            throw new NotSupportedException("native libapng not available in minimal build");
        }
    }
}

namespace Microsoft.Xna.Framework.Graphics
{
    public enum SurfaceFormat
    {
        Unknown = 0,
        Color = 2,
        Bgra4444 = 1,
        Bgra32 = 3,
        Bgr565 = 517,
        Dxt3 = 1026,
        Dxt5 = 2050
    }

    public class Texture2D
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}

namespace Microsoft.Xna.Framework
{
    public struct Point
    {
        public int X;
        public int Y;
        public Point(int x, int y) { X = x; Y = y; }
        public static Point Zero = new Point(0, 0);
    }

    public struct Rectangle
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public Rectangle(Point location, Point size)
        {
            X = location.X; Y = location.Y; Width = size.X; Height = size.Y;
        }
    }
}

namespace NAudio.Wave
{
    public enum WaveFormatEncoding : short
    {
        Pcm = 1,
        IeeeFloat = 3,
        MpegLayer3 = 0x55
    }

    public class WaveFormat
    {
        public int SampleRate;
        public int AverageBytesPerSecond;
        public int ExtraSize;
        public WaveFormatEncoding Encoding;
    }

    public class Mp3WaveFormat : WaveFormat
    {
    }
}
