using AttendAI.Application.FaceVerification;
using AttendAI.Domain.Enums;
using Microsoft.Extensions.Options;

namespace AttendAI.Infrastructure.FaceVerification;

public sealed class FaceVerificationPolicyService : IFaceVerificationPolicyService
{
    private readonly FaceVerificationOptions _options;

    public FaceVerificationPolicyService(IOptions<FaceVerificationOptions> options)
    {
        _options = options.Value;
    }

    public bool IsMatch(decimal score, decimal threshold, ScoreMetric metric)
        => metric switch
        {
            ScoreMetric.EuclideanDistance => score <= threshold,
            ScoreMetric.CosineSimilarity => score >= threshold,
            ScoreMetric.EngineDefined => score >= threshold,
            _ => score >= threshold
        };

    public FaceVerificationDecision Decide(decimal score, decimal threshold, ScoreMetric metric)
        => IsMatch(score, threshold, metric)
            ? FaceVerificationDecision.Match
            : FaceVerificationDecision.NoMatch;

    public decimal ConfiguredThreshold
        => _options.VerificationThreshold <= 0 ? 0.95m : (decimal)_options.VerificationThreshold;
}
