using System.Text;
using AttendAI.Application.FaceRecognition;
using AttendAI.Infrastructure.Configuration;
using AttendAI.Infrastructure.FaceRecognition;
using Microsoft.Extensions.Options;

namespace AttendAI.UnitTests.Application;

public sealed class FakeFaceRecognitionEngineTests
{
    private readonly FakeFaceRecognitionEngine _engine = new(Options.Create(new FaceRecognitionOptions
    {
        Provider = "Fake",
        Threshold = 0.95
    }));

    [Fact]
    public async Task ExtractEncodingAsync_rejects_empty_image()
    {
        var result = await _engine.ExtractEncodingAsync([]);

        Assert.False(result.Succeeded);
        Assert.Equal(FaceRecognitionErrorCode.EmptyImage, result.ErrorCode);
    }

    [Fact]
    public async Task ExtractEncodingAsync_rejects_no_face_marker()
    {
        var result = await _engine.ExtractEncodingAsync(Encoding.UTF8.GetBytes("NO_FACE"));

        Assert.False(result.Succeeded);
        Assert.Equal(FaceRecognitionErrorCode.NoFaceDetected, result.ErrorCode);
    }

    [Fact]
    public async Task ExtractEncodingAsync_rejects_multiple_face_marker()
    {
        var result = await _engine.ExtractEncodingAsync(Encoding.UTF8.GetBytes("MULTI_FACE"));

        Assert.False(result.Succeeded);
        Assert.Equal(FaceRecognitionErrorCode.MultipleFacesDetected, result.ErrorCode);
    }

    [Fact]
    public async Task VerifyAsync_matches_identical_payloads()
    {
        var imageBytes = Encoding.UTF8.GetBytes("approved-local-test-payload");
        var encoding = await _engine.ExtractEncodingAsync(imageBytes);

        var verification = await _engine.VerifyAsync(imageBytes, encoding.Template!);

        Assert.True(verification.Succeeded);
        Assert.True(verification.IsMatch);
        Assert.Equal(1, verification.SimilarityScore);
    }

    [Fact]
    public async Task VerifyAsync_does_not_match_different_payloads()
    {
        var first = Encoding.UTF8.GetBytes("approved-local-test-payload-a");
        var second = Encoding.UTF8.GetBytes("approved-local-test-payload-b");
        var encoding = await _engine.ExtractEncodingAsync(first);

        var verification = await _engine.VerifyAsync(second, encoding.Template!);

        Assert.True(verification.Succeeded);
        Assert.False(verification.IsMatch);
    }
}
