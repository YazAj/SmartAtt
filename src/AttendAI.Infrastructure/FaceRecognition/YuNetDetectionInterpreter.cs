using AttendAI.Application.FaceRecognition;
using AttendAI.Infrastructure.Configuration;
using OpenCvSharp;

namespace AttendAI.Infrastructure.FaceRecognition;

public static class YuNetDetectionInterpreter
{
    public static YuNetDetectionInterpretation Interpret(
        IReadOnlyList<float[]> rows,
        int imageWidth,
        int imageHeight,
        RealFaceRecognitionOptions options)
    {
        if (imageWidth <= 0 || imageHeight <= 0)
        {
            return YuNetDetectionInterpretation.Failure(
                FaceRecognitionErrorCode.VerificationFailed,
                "The decoded image dimensions are invalid.",
                rows.Count);
        }

        if (rows.Count == 0)
        {
            return YuNetDetectionInterpretation.Failure(
                FaceRecognitionErrorCode.NoFaceDetected,
                "No face was detected.",
                0);
        }

        var accepted = new List<YuNetFaceDetection>();
        FaceRecognitionErrorCode lastRejectedCode = FaceRecognitionErrorCode.NoFaceDetected;
        var lastRejectedMessage = "No accepted face was detected.";

        foreach (var row in rows)
        {
            if (row.Length < 15)
            {
                return YuNetDetectionInterpretation.Failure(
                    FaceRecognitionErrorCode.VerificationFailed,
                    "YuNet detector output did not contain the expected fields.",
                    rows.Count);
            }

            var confidence = row[14];
            if (!float.IsFinite(confidence) || confidence < options.DetectionScoreThreshold)
            {
                continue;
            }

            var box = ClampBox(row[0], row[1], row[2], row[3], imageWidth, imageHeight);
            if (box.Width <= 0 || box.Height <= 0)
            {
                lastRejectedCode = FaceRecognitionErrorCode.VerificationFailed;
                lastRejectedMessage = "YuNet detector returned an invalid face box.";
                continue;
            }

            var areaRatio = (box.Width * box.Height) / (double)(imageWidth * imageHeight);
            if (areaRatio < options.MinimumFaceAreaRatio)
            {
                lastRejectedCode = FaceRecognitionErrorCode.FaceTooSmall;
                lastRejectedMessage = "The detected face is too small.";
                continue;
            }

            if (areaRatio > options.MaximumFaceAreaRatio)
            {
                lastRejectedCode = FaceRecognitionErrorCode.FaceTooLarge;
                lastRejectedMessage = "The detected face is too large.";
                continue;
            }

            var landmarks = ReadLandmarks(row);
            if (!LandmarksAreValid(landmarks, imageWidth, imageHeight))
            {
                lastRejectedCode = FaceRecognitionErrorCode.InvalidLandmarks;
                lastRejectedMessage = "YuNet detector returned invalid landmarks.";
                continue;
            }

            accepted.Add(new YuNetFaceDetection(box, landmarks, confidence, areaRatio));
        }

        if (accepted.Count == 0)
        {
            return YuNetDetectionInterpretation.Failure(lastRejectedCode, lastRejectedMessage, rows.Count);
        }

        if (options.RequireExactlyOneFace && accepted.Count > 1)
        {
            return YuNetDetectionInterpretation.Failure(
                FaceRecognitionErrorCode.MultipleFacesDetected,
                "Multiple accepted faces were detected.",
                accepted.Count);
        }

        var selected = accepted
            .OrderByDescending(face => face.Confidence)
            .First();
        return YuNetDetectionInterpretation.Success(selected, accepted.Count);
    }

    private static Point2f[] ReadLandmarks(IReadOnlyList<float> row)
        =>
        [
            new(row[4], row[5]),
            new(row[6], row[7]),
            new(row[8], row[9]),
            new(row[10], row[11]),
            new(row[12], row[13])
        ];

    private static Rect2d ClampBox(float x, float y, float width, float height, int imageWidth, int imageHeight)
    {
        if (!float.IsFinite(x) || !float.IsFinite(y) || !float.IsFinite(width) || !float.IsFinite(height))
        {
            return new Rect2d(0, 0, 0, 0);
        }

        var left = Math.Clamp((double)x, 0, Math.Max(0, imageWidth - 1));
        var top = Math.Clamp((double)y, 0, Math.Max(0, imageHeight - 1));
        var right = Math.Clamp(x + width, 0, imageWidth);
        var bottom = Math.Clamp(y + height, 0, imageHeight);
        return new Rect2d(left, top, Math.Max(0, right - left), Math.Max(0, bottom - top));
    }

    private static bool LandmarksAreValid(IReadOnlyList<Point2f> landmarks, int imageWidth, int imageHeight)
        => landmarks.Count == 5 &&
           landmarks.All(point =>
               float.IsFinite(point.X) &&
               float.IsFinite(point.Y) &&
               point.X >= 0 &&
               point.Y >= 0 &&
               point.X < imageWidth &&
               point.Y < imageHeight);
}

public sealed record YuNetDetectionInterpretation(
    bool Succeeded,
    YuNetFaceDetection? Face,
    FaceRecognitionErrorCode ErrorCode,
    string Message,
    int FaceCount)
{
    public static YuNetDetectionInterpretation Success(YuNetFaceDetection face, int faceCount)
        => new(true, face, FaceRecognitionErrorCode.None, string.Empty, faceCount);

    public static YuNetDetectionInterpretation Failure(FaceRecognitionErrorCode errorCode, string message, int faceCount)
        => new(false, null, errorCode, message, faceCount);
}

public sealed record YuNetFaceDetection(
    Rect2d Box,
    IReadOnlyList<Point2f> Landmarks,
    double Confidence,
    double FaceAreaRatio);
