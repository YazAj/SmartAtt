namespace AttendAI.Application.FaceRecognition;

public sealed record FaceEncodingResult(
    bool Succeeded,
    StoredFaceTemplate? Template,
    FaceRecognitionErrorCode ErrorCode,
    string Message,
    int FaceCount = 0,
    double QualityScore = 0,
    double BrightnessScore = 0,
    double SharpnessScore = 0,
    double FaceAreaRatio = 0,
    double PoseScore = 0,
    string EngineName = "",
    string EngineVersion = "",
    string ModelName = "",
    string ModelVersion = "")
{
    public static FaceEncodingResult Success(StoredFaceTemplate template)
        => new(
            true,
            template,
            FaceRecognitionErrorCode.None,
            "Face template extracted.",
            1,
            template.QualityScore,
            0.8,
            0.8,
            0.25,
            0.9,
            template.EngineName,
            template.EngineVersion,
            template.ModelName,
            template.ModelVersion);

    public static FaceEncodingResult Failure(FaceRecognitionErrorCode errorCode, string message)
        => new(false, null, errorCode, message);
}
