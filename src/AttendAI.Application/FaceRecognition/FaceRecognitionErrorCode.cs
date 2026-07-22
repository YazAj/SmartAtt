namespace AttendAI.Application.FaceRecognition;

public enum FaceRecognitionErrorCode
{
    None = 0,
    EmptyImage = 1,
    NoFaceDetected = 2,
    MultipleFacesDetected = 3,
    TemplateUnavailable = 4,
    EngineUnavailable = 5,
    VerificationFailed = 6
}
