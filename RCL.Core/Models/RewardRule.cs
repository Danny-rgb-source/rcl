namespace RCL.Core.Models
{
    public class RewardRule
    {
        public enum RuleModeType
        {
            FixedVisits
        }

        public RuleModeType Mode { get; set; } = RuleModeType.FixedVisits;

        public int VisitsRequired { get; set; } = 5;

        public bool IsEligible(int visits) => visits >= VisitsRequired;

        public static RewardRule Default() => new RewardRule();

        public bool EvaluateEligibility(int visits, out string rewardDesc)
        {
            var eligible = IsEligible(visits);
            rewardDesc = eligible
                ? $"Eligible (>= {VisitsRequired} visits)"
                : $"Not eligible (< {VisitsRequired} visits)";
            return eligible;
        }
    }
}

