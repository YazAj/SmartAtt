namespace AttendAI.Domain.Enums;

public enum FaceCaptureRejectionReason
{
    None = 0,
    NoFace = 1,
    MultipleFaces = 2,
    ImageTooSmall = 3,
    ImageTooLarge = 4,
    UnsupportedFormat = 5,
    InvalidImage = 6,
    LowBrightness = 7,
    HighBrightness = 8,
    LowSharpness = 9,
    FaceTooSmall = 10,
    FaceTooLarge = 11,
    PoseRejected = 12,
    QualityTooLow = 13,
    EngineUnavailable = 14,
    ProcessingFailed = 15
}
