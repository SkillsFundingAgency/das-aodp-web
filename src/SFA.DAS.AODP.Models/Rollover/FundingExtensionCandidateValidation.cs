namespace SFA.DAS.AODP.Models.Rollover
{
    public class FundingExtensionCandidateValidation
    {
        public int ErrorCount { get; set; }
        public bool HasErrors => ErrorDetails.Any();
        public List<ErrorDetails> ErrorDetails { get; set; } = new();

        public void AddError(int rowNumber, string message)
        {
            ErrorDetails.Add(new ErrorDetails
            {
                RowNumber = rowNumber,
                ErrorMessage = message
            });
        }
    }
}
