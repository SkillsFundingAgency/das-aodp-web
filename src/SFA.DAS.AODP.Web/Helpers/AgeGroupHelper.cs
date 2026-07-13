using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Web.Helpers
{
    [ExcludeFromCodeCoverage]
    public static class AgeGroupHelper
    {
        public static string ToDisplay(this AgeGroup ageGroup)
        {
            return ageGroup switch
            {
                AgeGroup.Pre16 => "Pre-16",
                AgeGroup.SixteenToEighteen => "16 to 18",
                AgeGroup.EighteenPlus => "18+",
                AgeGroup.NineteenPlus => "19+",
                _ => ageGroup.ToString()
            };
        }

        public static IEnumerable<(AgeGroup Value, string Label)> GetOptions()
        {
            return Enum.GetValues<AgeGroup>()
                .Select(x => (x, x.ToDisplay()));
        }
    }
}
