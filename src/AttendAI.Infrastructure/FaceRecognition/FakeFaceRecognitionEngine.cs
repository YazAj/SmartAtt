using System.Security.Cryptography;
using System.Text;
using AttendAI.Application.FaceRecognition;
using AttendAI.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AttendAI.Infrastructure.FaceRecognition;

public sealed class FakeFaceRecognitionEngine : IFaceRecognitionEngine
{
    private const string TemplateVersion = "fake-sha256-v1";
    private const string TemplateFormat = "application/vnd.attendai.fake-face-template";
    private readonly double _threshold;
    private readonly FaceRecognitionOptions _options;

    public FakeFaceRecognitionEngine(IOptions<FaceRecognitionOptions> options)
    {
        _options = options.Value;
        _threshold = _options.Threshold <= 0 ? 0.95 : _options.Threshold;
    }

    public Task<FaceEncodingResult> ExtractEncodingAsync(
        byte[] imageBytes,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (imageBytes.Length == 0)
        {
            return Task.FromResult(FaceEncodingResult.Failure(
                FaceRecognitionErrorCode.EmptyImage,
                "The submitted image payload is empty."));
        }

        if (ContainsMarker(imageBytes, "NO_FACE"))
        {
            return Task.FromResult(FaceEncodingResult.Failure(
                FaceRecognitionErrorCode.NoFaceDetected,
                "The fake engine marker indicates that no face was detected."));
        }

        if (ContainsMarker(imageBytes, "MULTI_FACE"))
        {
            return Task.FromResult(FaceEncodingResult.Failure(
                FaceRecognitionErrorCode.MultipleFacesDetected,
                "The fake engine marker indicates that multiple faces were detected."));
        }

        var templateData = SHA256.HashData(imageBytes);
        var template = new StoredFaceTemplate(
            TemplateVersion,
            TemplateFormat,
            templateData,
            DateTimeOffset.UtcNow,
            "Fake",
            string.IsNullOrWhiteSpace(_options.EngineVersion) ? TemplateVersion : _options.EngineVersion,
            string.IsNullOrWhiteSpace(_options.ModelName) ? "Deterministic fake engine" : _options.ModelName,
            string.IsNullOrWhiteSpace(_options.ModelVersion) ? "v1" : _options.ModelVersion,
            templateData.Length,
            0.9);

        return Task.FromResult(FaceEncodingResult.Success(template));
    }

    public async Task<FaceVerificationResult> VerifyAsync(
        byte[] imageBytes,
        StoredFaceTemplate template,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (template.TemplateData.Length == 0)
        {
            return FaceVerificationResult.Failure(
                FaceRecognitionErrorCode.TemplateUnavailable,
                "The stored template payload is empty.");
        }

        var encodingResult = await ExtractEncodingAsync(imageBytes, cancellationToken);
        if (!encodingResult.Succeeded || encodingResult.Template is null)
        {
            return FaceVerificationResult.Failure(encodingResult.ErrorCode, encodingResult.Message);
        }

        var similarity = CalculateSimilarity(encodingResult.Template.TemplateData, template.TemplateData);
        return similarity >= _threshold
            ? FaceVerificationResult.Match(similarity)
            : FaceVerificationResult.NoMatch(similarity);
    }

    private static bool ContainsMarker(byte[] imageBytes, string marker)
    {
        var text = Encoding.UTF8.GetString(imageBytes);
        return text.Contains(marker, StringComparison.OrdinalIgnoreCase);
    }

    private static double CalculateSimilarity(byte[] currentTemplate, byte[] storedTemplate)
    {
        var length = Math.Min(currentTemplate.Length, storedTemplate.Length);
        if (length == 0)
        {
            return 0;
        }

        var matchingBytes = 0;
        for (var index = 0; index < length; index++)
        {
            if (currentTemplate[index] == storedTemplate[index])
            {
                matchingBytes++;
            }
        }

        return matchingBytes / (double)Math.Max(currentTemplate.Length, storedTemplate.Length);
    }
}
