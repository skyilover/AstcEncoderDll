
string inputFilePath = "input.bmp"; // Path to your BMP file
string outputFilePath = "output.astc"; // Path for the output ASTC file


uint targetWidth = 512;                  // 원하는 이미지 너비
uint targetHeight = 512;                 // 원하는 이미지 높이
uint blockX = 6;                         // 블록 크기 X
uint blockY = 6;                         // 블록 크기 Y
float quality = 60.0f;                   // 압축 품질 (

//ASTCENC_PRE_FASTEST = 0.0f;
//ASTCENC_PRE_FAST = 10.0f;
//ASTCENC_PRE_MEDIUM = 60.0f;
//ASTCENC_PRE_THOROUGH = 98.0f;
//ASTCENC_PRE_VERYTHOROUGH = 99.0f;
//ASTCENC_PRE_EXHAUSTIVE = 100.0f;

try
{
    ImageConverter.ConvertToAstc(inputFilePath, outputFilePath, targetWidth, targetHeight, blockX, blockY, quality);
    Console.WriteLine("Success! BMP converted to ASTC.");
}
catch (Exception ex)
{
    Console.WriteLine($"오류 발생: {ex.Message}");
}


