namespace AttendAI.Application.FaceRecognition;

public interface IFaceRecognitionEngine
{
    Task<FaceEncodingResult> ExtractEncodingAsync(
        byte[] imageBytes,
        CancellationToken cancellationToken = default);

    Task<FaceVerificationResult> VerifyAsync(
        byte[] imageBytes,
        StoredFaceTemplate template,
        CancellationToken cancellationToken = default);
}
