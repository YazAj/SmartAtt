namespace AttendAI.Domain.Enums;

public enum FaceVerificationDecision
{
    Unknown = 0,
    Match = 1,
    NoMatch = 2,
    NoFaceDetected = 3,
    MultipleFacesDetected = 4,
    EngineError = 5
}
