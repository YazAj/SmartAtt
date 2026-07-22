using AttendAI.Application.FaceRecognition;
using AttendAI.Infrastructure.Configuration;
using AttendAI.Infrastructure.FaceRecognition;
using Microsoft.Extensions.Options;

var arguments = args
    .Where(argument => !argument.StartsWith("--", StringComparison.Ordinal))
    .ToArray();

if (arguments.Length is < 1 or > 2)
{
    PrintUsage();
    return 2;
}

var engine = new FakeFaceRecognitionEngine(Options.Create(new FaceRecognitionOptions
{
    Provider = "Fake",
    Threshold = 0.95
}));

try
{
    if (arguments.Length == 1)
    {
        var imageBytes = await LoadImageAsync(arguments[0]);
        var result = await engine.ExtractEncodingAsync(imageBytes);
        PrintEncodingResult(arguments[0], result);
        return result.Succeeded ? 0 : 1;
    }

    var firstImageBytes = await LoadImageAsync(arguments[0]);
    var secondImageBytes = await LoadImageAsync(arguments[1]);

    var firstEncoding = await engine.ExtractEncodingAsync(firstImageBytes);
    if (!firstEncoding.Succeeded || firstEncoding.Template is null)
    {
        PrintEncodingResult(arguments[0], firstEncoding);
        return 1;
    }

    var verification = await engine.VerifyAsync(secondImageBytes, firstEncoding.Template);
    PrintVerificationResult(arguments[0], arguments[1], verification);
    return verification.Succeeded ? 0 : 1;
}
catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
{
    Console.Error.WriteLine($"File error: {exception.Message}");
    return 1;
}

static async Task<byte[]> LoadImageAsync(string path)
{
    if (!File.Exists(path))
    {
        throw new FileNotFoundException($"Image file was not found: {path}", path);
    }

    return await File.ReadAllBytesAsync(path);
}

static void PrintEncodingResult(string imagePath, FaceEncodingResult result)
{
    Console.WriteLine($"Image: {imagePath}");
    Console.WriteLine($"Succeeded: {result.Succeeded}");
    Console.WriteLine($"Code: {result.ErrorCode}");
    Console.WriteLine($"Message: {result.Message}");

    if (result.Template is not null)
    {
        Console.WriteLine($"Template version: {result.Template.Version}");
        Console.WriteLine($"Template format: {result.Template.Format}");
        Console.WriteLine($"Template bytes: {result.Template.TemplateData.Length}");
    }
}

static void PrintVerificationResult(string firstImagePath, string secondImagePath, FaceVerificationResult result)
{
    Console.WriteLine($"Reference image: {firstImagePath}");
    Console.WriteLine($"Probe image: {secondImagePath}");
    Console.WriteLine($"Succeeded: {result.Succeeded}");
    Console.WriteLine($"Is match: {result.IsMatch}");
    Console.WriteLine($"Similarity: {result.SimilarityScore:0.000}");
    Console.WriteLine($"Code: {result.ErrorCode}");
    Console.WriteLine($"Message: {result.Message}");
}

static void PrintUsage()
{
    Console.WriteLine("AttendAI Face Recognition POC");
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project experiments/AttendAI.FaceRecognition.Poc -- <image-path>");
    Console.WriteLine("  dotnet run --project experiments/AttendAI.FaceRecognition.Poc -- <reference-image-path> <probe-image-path>");
    Console.WriteLine();
    Console.WriteLine("Sprint 1 uses the deterministic fake engine by default.");
    Console.WriteLine("For negative test paths, provide a text file containing NO_FACE or MULTI_FACE as the payload marker.");
}
