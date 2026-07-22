using AttendAI.Application.FaceVerification;
using AttendAI.Domain.Enums;
using AttendAI.Infrastructure.FaceVerification;
using Microsoft.Extensions.Options;

namespace AttendAI.UnitTests.Application;

public sealed class FaceVerificationPolicyTests
{
    [Fact]
    public void Decide_matches_cosine_similarity_at_or_above_threshold()
    {
        var policy = new FaceVerificationPolicyService(Options.Create(new FaceVerificationOptions()));

        Assert.Equal(FaceVerificationDecision.Match, policy.Decide(0.95m, 0.95m, ScoreMetric.CosineSimilarity));
        Assert.Equal(FaceVerificationDecision.NoMatch, policy.Decide(0.94m, 0.95m, ScoreMetric.CosineSimilarity));
    }

    [Fact]
    public void Decide_matches_euclidean_distance_at_or_below_threshold()
    {
        var policy = new FaceVerificationPolicyService(Options.Create(new FaceVerificationOptions()));

        Assert.Equal(FaceVerificationDecision.Match, policy.Decide(0.40m, 0.50m, ScoreMetric.EuclideanDistance));
        Assert.Equal(FaceVerificationDecision.NoMatch, policy.Decide(0.60m, 0.50m, ScoreMetric.EuclideanDistance));
    }
}
