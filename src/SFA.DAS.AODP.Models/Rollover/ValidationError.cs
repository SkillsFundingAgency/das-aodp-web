using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Models.Rollover
{
    [ExcludeFromCodeCoverage]
    public class ValidationError
    {
        public string Field { get; set; }
        public string Message { get; set; }
    }
}
