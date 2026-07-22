using AttendAI.Application.Biometrics;
using AttendAI.Domain.Enums;
using Microsoft.Extensions.Options;

namespace AttendAI.Infrastructure.Biometrics;

public sealed class FaceCaptureValidator : IFaceCaptureValidator
{
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private readonly BiometricEnrollmentOptions _options;

    public FaceCaptureValidator(IOptions<BiometricEnrollmentOptions> options)
    {
        _options = options.Value;
    }

    public ImageValidationResult Validate(FaceCaptureSample sample)
    {
        if (sample.ImageBytes.Length == 0)
        {
            return ImageValidationResult.Failure(FaceCaptureRejectionReason.InvalidImage, "ErrorBiometricImageEmpty");
        }

        if (sample.ImageBytes.Length > _options.MaximumCaptureBytes)
        {
            return ImageValidationResult.Failure(FaceCaptureRejectionReason.ImageTooLarge, "ErrorBiometricImageTooLarge");
        }

        var contentType = (sample.ContentType ?? string.Empty).Trim().ToLowerInvariant();
        if (!_options.AllowedMimeTypes.Any(item => string.Equals(item, contentType, StringComparison.OrdinalIgnoreCase)))
        {
            return ImageValidationResult.Failure(FaceCaptureRejectionReason.UnsupportedFormat, "ErrorBiometricUnsupportedFormat");
        }

        var dimensions = contentType switch
        {
            "image/png" => TryReadPngDimensions(sample.ImageBytes),
            "image/jpeg" => TryReadJpegDimensions(sample.ImageBytes),
            _ => null
        };

        if (dimensions is null)
        {
            return ImageValidationResult.Failure(FaceCaptureRejectionReason.InvalidImage, "ErrorBiometricInvalidImage");
        }

        var (width, height) = dimensions.Value;
        if (width < _options.MinimumImageWidth || height < _options.MinimumImageHeight)
        {
            return ImageValidationResult.Failure(FaceCaptureRejectionReason.ImageTooSmall, "ErrorBiometricImageTooSmall");
        }

        if (width > _options.MaximumImageWidth || height > _options.MaximumImageHeight)
        {
            return ImageValidationResult.Failure(FaceCaptureRejectionReason.ImageTooLarge, "ErrorBiometricImageDimensionsTooLarge");
        }

        return ImageValidationResult.Success(width, height, sample.ImageBytes.LongLength);
    }

    private static (int Width, int Height)? TryReadPngDimensions(byte[] bytes)
    {
        if (bytes.Length < 24 || !PngSignature.SequenceEqual(bytes.Take(PngSignature.Length)))
        {
            return null;
        }

        var width = ReadInt32BigEndian(bytes, 16);
        var height = ReadInt32BigEndian(bytes, 20);
        return width > 0 && height > 0 ? (width, height) : null;
    }

    private static (int Width, int Height)? TryReadJpegDimensions(byte[] bytes)
    {
        if (bytes.Length < 4 || bytes[0] != 0xFF || bytes[1] != 0xD8)
        {
            return null;
        }

        var index = 2;
        while (index + 9 < bytes.Length)
        {
            if (bytes[index] != 0xFF)
            {
                index++;
                continue;
            }

            var marker = bytes[index + 1];
            index += 2;
            if (marker is 0xD8 or 0xD9)
            {
                continue;
            }

            if (index + 2 > bytes.Length)
            {
                return null;
            }

            var length = (bytes[index] << 8) + bytes[index + 1];
            if (length < 2 || index + length > bytes.Length)
            {
                return null;
            }

            if (marker is 0xC0 or 0xC1 or 0xC2 or 0xC3 or 0xC5 or 0xC6 or 0xC7 or 0xC9 or 0xCA or 0xCB or 0xCD or 0xCE or 0xCF)
            {
                var height = (bytes[index + 3] << 8) + bytes[index + 4];
                var width = (bytes[index + 5] << 8) + bytes[index + 6];
                return width > 0 && height > 0 ? (width, height) : null;
            }

            index += length;
        }

        return null;
    }

    private static int ReadInt32BigEndian(byte[] bytes, int offset)
        => (bytes[offset] << 24) |
           (bytes[offset + 1] << 16) |
           (bytes[offset + 2] << 8) |
           bytes[offset + 3];
}
