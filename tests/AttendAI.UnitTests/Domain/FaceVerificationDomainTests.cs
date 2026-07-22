using AttendAI.Domain.Academic;
using AttendAI.Domain.Enums;

namespace AttendAI.UnitTests.Domain;

public sealed class FaceVerificationDomainTests
{
    [Fact]
    public void FaceVerificationAttempt_stores_safe_metadata_only()
    {
        var attempt = new FaceVerificationAttempt(
            Guid.NewGuid(),
            Guid.NewGuid(),
            FaceVerificationPurpose.SelfTest,
            FaceVerificationOutcome.Matched,
            FaceVerificationDecision.Match,
            null,
            1m,
            0.95m,
            ScoreMetric.CosineSimilarity,
            "Fake",
            "fake-sha256-v1",
            "Deterministic fake engine",
            "v1",
            "fake-sha256-v1",
            DateTimeOffset.UtcNow,
            "request-1",
            "FaceVerificationAttemptDescriptionMatched",
            160,
            120,
            1,
            0.9m,
            12);

        Assert.Equal(FaceVerificationOutcome.Matched, attempt.Outcome);
        Assert.Equal(FaceVerificationDecision.Match, attempt.Decision);
        Assert.Equal("FaceVerificationAttemptDescriptionMatched", attempt.SafeDescription);
    }

    [Fact]
    public void FaceVerificationAttempt_rejects_negative_score()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FaceVerificationAttempt(
            Guid.NewGuid(),
            null,
            FaceVerificationPurpose.SelfTest,
            FaceVerificationOutcome.NotMatched,
            FaceVerificationDecision.NoMatch,
            null,
            -0.1m,
            0.95m,
            ScoreMetric.CosineSimilarity,
            "Fake",
            "fake-sha256-v1",
            "Deterministic fake engine",
            "v1",
            "fake-sha256-v1",
            DateTimeOffset.UtcNow,
            null,
            "FaceVerificationAttemptDescriptionNotMatched"));
    }
}
