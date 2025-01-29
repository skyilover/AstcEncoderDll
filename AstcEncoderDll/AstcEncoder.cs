using System;
using System.Runtime.InteropServices;

public class AstcEncoder
{
    public enum AstcencProfile
    {
        LdrSrgb = 0,         // ASTCENC_PRF_LDR_SRGB
        Ldr = 1,             // ASTCENC_PRF_LDR
        HdrRgbLdrA = 2,      // ASTCENC_PRF_HDR_RGB_LDR_A
        Hdr = 3              // ASTCENC_PRF_HDR
    }


    public enum AstcencError
    {
        Success = 0,               // ASTCENC_SUCCESS
        OutOfMemory,               // ASTCENC_ERR_OUT_OF_MEM
        BadCpuFloat,               // ASTCENC_ERR_BAD_CPU_FLOAT
        BadParam,                  // ASTCENC_ERR_BAD_PARAM
        BadBlockSize,              // ASTCENC_ERR_BAD_BLOCK_SIZE
        BadProfile,                // ASTCENC_ERR_BAD_PROFILE
        BadQuality,                // ASTCENC_ERR_BAD_QUALITY
        BadSwizzle,                // ASTCENC_ERR_BAD_SWIZZLE
        BadFlags,                  // ASTCENC_ERR_BAD_FLAGS
        BadContext,                // ASTCENC_ERR_BAD_CONTEXT
        NotImplemented             // ASTCENC_ERR_NOT_IMPLEMENTED
    }
    
    public enum AstcencType
    {
        U8 = 0,  // Unsigned 8-bit integer
        F16 = 1, // Half-precision floating point (16-bit)
        F32 = 2  // Full-precision floating point (32-bit)
    }
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void AstcencProgressCallback(float progress, IntPtr userData);

    [StructLayout(LayoutKind.Sequential)]
    public struct AstcencConfig
    {
        public AstcencProfile Profile;     // Color profile (enum)
        public uint Flags;                 // Compression flags (bitfield)
        public uint BlockX;                // Block size X dimension
        public uint BlockY;                // Block size Y dimension
        public uint BlockZ;                // Block size Z dimension

        public float CwRWeight;            // Red component weight scale for error weighting
        public float CwGWeight;            // Green component weight scale for error weighting
        public float CwBWeight;            // Blue component weight scale for error weighting
        public float CwAWeight;            // Alpha component weight scale for error weighting

        // Alpha scale radius (투명도 처리용)
        public uint AScaleRadius;

        // RGBM 스케일 팩터
        public float RgbmMScale;

        // 튜닝 파라미터: 파티션 제한
        public uint TunePartitionCountLimit;
        public uint Tune2PartitionIndexLimit;
        public uint Tune3PartitionIndexLimit;
        public uint Tune4PartitionIndexLimit;

        // 튜닝 파라미터: 블록 모드 제한 및 정제 단계
        public uint TuneBlockModeLimit;
        public uint TuneRefinementLimit;

        public uint tune_candidate_limit;
        public uint tune_2partitioning_candidate_limit;
        public uint tune_3partitioning_candidate_limit;
        public uint tune_4partitioning_candidate_limit;

        // 튜닝 파라미터: MSE 한계값
        public float TuneDbLimit;
        
        public float tune_mse_overshoot;
        public float tune_2partition_early_out_limit_factor;
        public float tune_3partition_early_out_limit_factor;
        public float tune_2plane_early_out_limit_correlation;
        public float tune_search_mode0_enable;
        public AstcencProgressCallback progress_callback;
        
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AstcencSwizzle
    {
        public int R;   // Red swizzle selector (0-5)
        public int G;   // Green swizzle selector (0-5)
        public int B;   // Blue swizzle selector (0-5)
        public int A;   // Alpha swizzle selector (0-5)
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AstcencImage
    {
        public uint DimX;           // Image width in texels
        public uint DimY;           // Image height in texels
        public uint DimZ;           // Image depth in texels (1 for 2D images)
        
        public AstcencType DataType;        // Data type of each component (e.g., U8, F16)
        
        public IntPtr Data;         // Pointer to image data (array of slices)
    }

    private const string DllName = "astcenc-avx2-shared.dll"; // DLL 파일 이름

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern AstcencError astcenc_config_init(
        AstcencProfile profile,
        uint blockX,
        uint blockY,
        uint blockZ,
        float quality,
        uint flags,
        ref AstcencConfig config);


    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern AstcencError astcenc_context_alloc(
        ref AstcencConfig config,
        uint threadCount,
        out IntPtr context);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern AstcencError astcenc_compress_image(
        IntPtr context,
        ref AstcencImage image,
        ref AstcencSwizzle swizzle,
        byte[] dataOut,
        ulong dataLen,
        uint threadIndex);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int astcenc_compress_reset(IntPtr context);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int astcenc_compress_cancel(IntPtr context);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int astcenc_decompress_image(
        IntPtr context, 
        byte[] data, 
        ulong dataLen, 
        IntPtr imageOut, 
        IntPtr swizzle, 
        uint threadIndex);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int astcenc_decompress_reset(IntPtr context);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void astcenc_context_free(IntPtr context);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int astcenc_get_block_info(
        IntPtr context, 
        byte[] data, 
        IntPtr info);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr astcenc_get_error_string(int status);
    
    
    
    public static void SaveAstcFile(string outputPath, byte[] compressedData, uint blockX, uint blockY, uint width, uint height)
    {
        using (var fileStream = new FileStream(outputPath, FileMode.Create))
        {
            // Step 1: ASTC 헤더 작성
            byte[] header = new byte[16];

            // 매직 넘버 (0x13AB A15C)
            header[0] = 0x13;
            header[1] = 0xAB;
            header[2] = 0xA1;
            header[3] = 0x5C;

            // 블록 크기
            header[4] = (byte)blockX; // X
            header[5] = (byte)blockY; // Y
            header[6] = 1; // Z (2D 이미지이므로 1)

            // 이미지 크기 (24비트 리틀 엔디안)
            header[7] = (byte)(width & 0xFF);
            header[8] = (byte)((width >> 8) & 0xFF);
            header[9] = (byte)((width >> 16) & 0xFF);

            header[10] = (byte)(height & 0xFF);
            header[11] = (byte)((height >> 8) & 0xFF);
            header[12] = (byte)((height >> 16) & 0xFF);

            header[13] = 1; // Z 크기 (2D 이미지이므로 항상 1)
            header[14] = 0;
            header[15] = 0;

            // Step 2: 헤더 쓰기
            fileStream.Write(header, 0, header.Length);

            // Step 3: 압축 데이터 쓰기
            fileStream.Write(compressedData, 0, compressedData.Length);
        }
    }
}


