namespace SFA.DAS.AODP.Web.Models.GdsComponents
{
    public class GdsDateInputViewModel
    {
        public string Title { get; set; }
        public string Hint { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public string? ErrorMessage { get; set; }
        public DateOnly? Value { get; set; }
        public bool ReadOnly { get; set; }
    }
}
