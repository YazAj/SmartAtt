namespace AttendAI.Application.FaceRecognition;

public sealed record FaceEncodingResult(
    bool Succeeded,
    StoredFaceTemplate? Template,
    FaceRecognitionErrorCode ErrorCode,
    string Message)
{
    public static FaceEncodingResult Success(StoredFaceTemplate template)
        => new(true, template, FaceRecognitionErrorCode.None, "Face template extracted.");

    public static FaceEncodingResult Failure(FaceRecognitionErrorCode errorCode, string message)
        => new(false, null, errorCode, message);
}
