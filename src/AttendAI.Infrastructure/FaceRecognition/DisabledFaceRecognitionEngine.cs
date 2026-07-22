using AttendAI.Application.FaceRecognition;

namespace AttendAI.Infrastructure.FaceRecognition;

public sealed class DisabledFaceRecognitionEngine : IFaceRecognitionEngine
{
    public Task<FaceEncodingResult> ExtractEncodingAsync(
        byte[] imageBytes,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(FaceEncodingResult.Failure(
            FaceRecognitionErrorCode.EngineUnavailable,
            "Face-recognition enrollment is disabled or no real engine adapter is configured."));
    }

    public Task<FaceVerificationResult> VerifyAsync(
        byte[] imageBytes,
        StoredFaceTemplate template,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(FaceVerificationResult.Failure(
            FaceRecognitionErrorCode.EngineUnavailable,
            "Face-recognition verification is disabled or no real engine adapter is configured."));
    }
}
