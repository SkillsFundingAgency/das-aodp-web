namespace SFA.DAS.AODP.Web.Models.GdsComponents
{
    public class PaginationModel
    {
        public int MaxPageNumber { get; set; }
        public int CurrentPage { get; set; }
        public string ActionName { get; set; } 
        public string ControllerName { get; set; }
        public Dictionary<string, object?> RouteValues { get; set; } = new();
    }
}
