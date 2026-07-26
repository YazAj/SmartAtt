namespace AttendAI.Infrastructure.FaceRecognition;

public static class RealFaceRecognitionMetadata
{
    public const string Mode = "Real";
    public const string EngineName = "OpenCV-SFace";
    public const string TemplateFormat = "application/vnd.attendai.opencv-sface-template";
    public const string TemplateFormatVersion = "opencv-sface-f32le-v1";
    public const string ScoreMetric = "CosineSimilarity";
    public const int AlignedFaceWidth = 112;
    public const int AlignedFaceHeight = 112;
    public const int SFaceEmbeddingDimension = 128;
}

public sealed record RealFaceModelPaths(
    string ApprovedModelRootPath,
    string DetectorModelPath,
    string RecognizerModelPath,
    string DetectorSafeIdentifier,
    string RecognizerSafeIdentifier);

public sealed record RealFaceModelReadiness(
    bool Succeeded,
    string StatusMessageKey,
    string GateResult,
    string ErrorCode,
    string OpenCvVersion,
    RealFaceModelPaths? Paths,
    RealFaceRecognizerTensorMetadata? RecognizerMetadata = null);

public sealed record RealFaceRecognizerTensorMetadata(
    string InputElementType,
    string InputShape,
    string OutputElementType,
    string OutputShape,
    int EmbeddingDimension);
