namespace AttendAI.Infrastructure.Configuration;

public sealed class FaceRecognitionOptions
{
    public const string SectionName = "FaceRecognition";

    public string Provider { get; set; } = "Fake";

    public string ModelPath { get; set; } = string.Empty;

    public string EngineVersion { get; set; } = "fake-sha256-v1";

    public string ModelName { get; set; } = "Deterministic fake engine";

    public string ModelVersion { get; set; } = "v1";

    public double Threshold { get; set; } = 0.95;

    public RealFaceRecognitionOptions RealEngine { get; set; } = new();
}

public sealed class RealFaceRecognitionOptions
{
    public const string ProviderName = "OpenCvSFace";

    public string Provider { get; set; } = ProviderName;

    public string ApprovedModelRoot { get; set; } = "../../models/face-recognition";

    public string DetectorModelPath { get; set; } = "../../models/face-recognition/yunet/face_detection_yunet_2023mar.onnx";

    public string RecognizerModelPath { get; set; } = "../../models/face-recognition/sface/face_recognition_sface_2021dec.onnx";

    public string DetectorModelSha256 { get; set; } = "8f2383e4dd3cfbb4553ea8718107fc0423210dc964f9f4280604804ed2552fa4";

    public string RecognizerModelSha256 { get; set; } = "0ba9fbfa01b5270c96627c4ef784da859931e02f04419c829e83484087c34e79";

    public int DetectorInputWidth { get; set; } = 320;

    public int DetectorInputHeight { get; set; } = 320;

    public double DetectionScoreThreshold { get; set; } = 0.90;

    public double DetectionNmsThreshold { get; set; } = 0.30;

    public int DetectionTopK { get; set; } = 5000;

    public double CosineSimilarityThreshold { get; set; } = 0.363;

    public string ThresholdStatus { get; set; } = "DevelopmentDefault";

    public double MinimumFaceAreaRatio { get; set; } = 0.01;

    public double MaximumFaceAreaRatio { get; set; } = 0.80;

    public double MinimumBrightnessScore { get; set; } = 0.18;

    public double MaximumBrightnessScore { get; set; } = 0.95;

    public double MinimumSharpnessScore { get; set; } = 0.05;

    public bool RequireExactlyOneFace { get; set; } = true;

    public string EngineName { get; set; } = "OpenCV-SFace";

    public string EngineVersion { get; set; } = "OpenCvSharp4 4.13.0.20260627 / OpenCV 4.13.0";

    public string DetectorName { get; set; } = "YuNet";

    public string DetectorVersion { get; set; } = "2023mar";

    public string RecognizerName { get; set; } = "SFace";

    public string RecognizerVersion { get; set; } = "2021dec";

    public string ModelName { get; set; } = "SFace";

    public string ModelVersion { get; set; } = "2021dec";

    public string TemplateFormatVersion { get; set; } = "opencv-sface-f32le-v1";

    public int EmbeddingDimension { get; set; } = 128;

    public bool EnableDevelopmentDiagnostics { get; set; }
}
