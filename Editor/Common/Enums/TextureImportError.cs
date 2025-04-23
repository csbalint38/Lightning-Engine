using System.ComponentModel;

namespace Editor.Common.Enums
{
    public enum TextureImportError
    {
        [Description("Import succeeded")]
        SUCCEEDED = 0,

        [Description("Unknown error")]
        UNKNOWN,

        [Description("Texture compression failed")]
        COMPRESS,

        [Description("Texture decompression failed")]
        DECOMPRESS,

        [Description("Failed to load the texture into memory")]
        LOAD,

        [Description("Texture mipmap generation failed")]
        MIPMAP_GENERATION,

        [Description("Maximum subresource size of 4GB exceeded")]
        MAX_SIZE_EXCEEDED,

        [Description("Source images don't have the same dimension")]
        SIZE_MISMATCH,

        [Description("Source images don't have the same format")]
        FORMAT_MISMATCH,

        [Description("Source image file not found")]
        FILE_NOT_FOUND,

        [Description(
            "Number of images for cubemaps should be a multiple of 6, or the source images should be equirectangular images with the same size and format."
        )]
        NEED_6_IMAGES
    }
}
