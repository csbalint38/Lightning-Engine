using Editor.Content;
using System.Runtime.InteropServices;

namespace Editor.DLLs.Descriptors
{
    [StructLayout(LayoutKind.Sequential)]
    public class TextureImportSettings
    {
        public string Sources;
        public int SourceCount;
        public int Dimension;
        public int MipLevels;
        public float AlphaThreshold;
        public int PreferBC7;
        public int OutputFormat;
        public int Compress;
        public int CubemapSize;
        public int MirrorCubemap;
        public int PrefilterCubemap;

        public void FromContentSettings(Texture texture)
        {
            var settings = texture.ImportSettings;

            Sources = string.Join(';', settings.Sources);
            SourceCount = settings.Sources.Count;
            Dimension = (int)settings.Dimension;
            MipLevels = settings.MipLevels;
            AlphaThreshold = settings.AlphaThreshold;
            PreferBC7 = settings.PreferBC7 ? 1 : 0;
            OutputFormat = (int)settings.OutputFormat;
            Compress = settings.Compress ? 1 : 0;
            CubemapSize = settings.CubemapSize;
            MirrorCubemap = settings.MirrorCubemap ? 1 : 0;
            PrefilterCubemap = settings.PrefilterCubemap ? 1 : 0;
        }
    }
}
