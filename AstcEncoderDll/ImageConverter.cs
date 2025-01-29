using System;
using System.IO;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

public class ImageConverter
{
    public static void ConvertToAstc(
        string inputFilePath,
        string outputFilePath,
        uint targetWidth,
        uint targetHeight,
        uint blockX,
        uint blockY,
        float quality)
    {
        byte[] pixelData;
        uint width = targetWidth;
        uint height = targetHeight;

        // Step 1: Load BMP file using ImageSharp and resize
        using (Image<Rgba32> bmp = Image.Load<Rgba32>(inputFilePath))
        {
            bmp.Mutate(x => x.Resize((int)width, (int)height, KnownResamplers.Bicubic));

            // Step 2: Extract pixel data (RGBA U8 format)
            pixelData = new byte[width * height * 4]; // 4 bytes per pixel (RGBA)
            int index = 0;

            bmp.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgba32> rowSpan = accessor.GetRowSpan(accessor.Height - 1 - y); // Flip vertically
                    foreach (Rgba32 pixel in rowSpan)
                    {
                        pixelData[index++] = pixel.R; // Red
                        pixelData[index++] = pixel.G; // Green
                        pixelData[index++] = pixel.B; // Blue
                        pixelData[index++] = pixel.A; // Alpha
                    }
                }
            });
        }

        // Step 3: Initialize ASTC encoder configuration
        var config = new AstcEncoder.AstcencConfig();
        var profile = AstcEncoder.AstcencProfile.Ldr;
        var blockZ = 1u; // For 2D images, Z is always 1
        var flags = 0u;

        var result = AstcEncoder.astcenc_config_init(profile, blockX, blockY, blockZ, quality, flags, ref config);
        if (result != AstcEncoder.AstcencError.Success)
        {
            throw new InvalidOperationException("Failed to initialize ASTC config.");
        }

        IntPtr context;
        result = AstcEncoder.astcenc_context_alloc(ref config, 1, out context);
        if (result != AstcEncoder.AstcencError.Success)
        {
            throw new InvalidOperationException("Failed to allocate ASTC context.");
        }

        // Step 4: Prepare image structure for ASTC encoder
        IntPtr slicePointer = Marshal.AllocHGlobal(pixelData.Length);
        Marshal.Copy(pixelData, 0, slicePointer, pixelData.Length);

        IntPtr[] slices = new IntPtr[] { slicePointer };
        IntPtr dataPointer = Marshal.AllocHGlobal(IntPtr.Size * slices.Length);
        Marshal.Copy(slices, 0, dataPointer, slices.Length);

        var image = new AstcEncoder.AstcencImage
        {
            DimX = width,
            DimY = height,
            DimZ = 1,
            DataType = AstcEncoder.AstcencType.U8,
            Data = dataPointer
        };

        var swizzle = new AstcEncoder.AstcencSwizzle { R = 0, G = 1, B = 2, A = 3 }; // Default RGBA swizzle

        uint xblocks = (image.DimX + blockX - 1) / blockX;
        uint yblocks = (image.DimY + blockY - 1) / blockY;
        uint zblocks = (image.DimZ + blockZ - 1) / blockZ;

        ulong dataLen = (ulong)(xblocks * yblocks * zblocks * 16); // ASTC block size is always 16 bytes
        byte[] compressedData = new byte[dataLen];

        // Step 5: Compress image using ASTC encoder
        result = AstcEncoder.astcenc_compress_image(context, ref image, ref swizzle, compressedData, dataLen, 0);
        if (result != AstcEncoder.AstcencError.Success)
        {
            Cleanup(slices, dataPointer);
            AstcEncoder.astcenc_context_free(context);
            throw new InvalidOperationException($"Failed to compress image: {result}");
        }

        try
        {
            // Save the compressed data as an ASTC file
            AstcEncoder.SaveAstcFile(outputFilePath, compressedData, blockX, blockY, width, height);
        }
        finally
        {
            // Cleanup allocated memory and free context
            Cleanup(slices, dataPointer);
            AstcEncoder.astcenc_context_free(context);
        }
    }

    private static void Cleanup(IntPtr[] slices, IntPtr dataPointer)
    {
        foreach (var slice in slices)
            Marshal.FreeHGlobal(slice);

        Marshal.FreeHGlobal(dataPointer);
    }
}
