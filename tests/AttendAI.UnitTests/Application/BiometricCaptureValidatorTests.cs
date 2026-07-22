using AttendAI.Application.Biometrics;
using AttendAI.Domain.Enums;
using AttendAI.Infrastructure.Biometrics;
using Microsoft.Extensions.Options;

namespace AttendAI.UnitTests.Application;

public sealed class BiometricCaptureValidatorTests
{
    private readonly FaceCaptureValidator _validator = new(Options.Create(new BiometricEnrollmentOptions()));

    [Fact]
    public void Validate_accepts_supported_png_within_dimensions()
    {
        var result = _validator.Validate(new FaceCaptureSample(Png(160, 120), "image/png", "captures"));

        Assert.True(result.Succeeded);
        Assert.Equal(160, result.Width);
        Assert.Equal(120, result.Height);
    }

    [Fact]
    public void Validate_rejects_unsupported_content_type()
    {
        var result = _validator.Validate(new FaceCaptureSample(Png(160, 120), "image/gif", "captures"));

        Assert.False(result.Succeeded);
        Assert.Equal(FaceCaptureRejectionReason.UnsupportedFormat, result.RejectionReason);
    }

    [Fact]
    public void Validate_rejects_image_below_minimum_dimensions()
    {
        var result = _validator.Validate(new FaceCaptureSample(Png(40, 40), "image/png", "captures"));

        Assert.False(result.Succeeded);
        Assert.Equal(FaceCaptureRejectionReason.ImageTooSmall, result.RejectionReason);
    }

    internal static byte[] Png(int width, int height, string? marker = null)
    {
        var bytes = new List<byte>
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
            0x00, 0x00, 0x00, 0x0D,
            0x49, 0x48, 0x44, 0x52
        };
        bytes.AddRange(Int32BigEndian(width));
        bytes.AddRange(Int32BigEndian(height));
        bytes.AddRange([0x08, 0x02, 0x00, 0x00, 0x00]);
        if (!string.IsNullOrWhiteSpace(marker))
        {
            bytes.AddRange(System.Text.Encoding.UTF8.GetBytes(marker));
        }

        return bytes.ToArray();
    }

    private static byte[] Int32BigEndian(int value)
        => [(byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value];
}
