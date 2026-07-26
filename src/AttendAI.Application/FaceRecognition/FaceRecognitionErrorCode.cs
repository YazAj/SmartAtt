namespace AttendAI.Application.FaceRecognition;

public enum FaceRecognitionErrorCode
{
    None = 0,
    EmptyImage = 1,
    NoFaceDetected = 2,
    MultipleFacesDetected = 3,
    TemplateUnavailable = 4,
    EngineUnavailable = 5,
    VerificationFailed = 6,
    InvalidImage = 7,
    InvalidLandmarks = 8,
    InvalidEmbedding = 9,
    ModelInvalid = 10,
    LowBrightness = 11,
    HighBrightness = 12,
    LowSharpness = 13,
    FaceTooSmall = 14,
    FaceTooLarge = 15
}
