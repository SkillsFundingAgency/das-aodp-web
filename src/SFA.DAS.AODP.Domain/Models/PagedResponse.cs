namespace SFA.DAS.AODP.Domain.Models;

public class PagedResponse<T>
{
    public List<T> Data { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
}