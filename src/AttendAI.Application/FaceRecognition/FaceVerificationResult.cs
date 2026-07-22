namespace AttendAI.Application.FaceRecognition;

public sealed record FaceVerificationResult(
    bool Succeeded,
    bool IsMatch,
    double SimilarityScore,
    FaceRecognitionErrorCode ErrorCode,
    string Message)
{
    public static FaceVerificationResult Match(double similarityScore)
        => new(true, true, similarityScore, FaceRecognitionErrorCode.None, "Face verification matched the stored template.");

    public static FaceVerificationResult NoMatch(double similarityScore)
        => new(true, false, similarityScore, FaceRecognitionErrorCode.None, "Face verification did not match the stored template.");

    public static FaceVerificationResult Failure(FaceRecognitionErrorCode errorCode, string message)
        => new(false, false, 0, errorCode, message);
}
