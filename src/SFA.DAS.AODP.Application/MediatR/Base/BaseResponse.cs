namespace SFA.DAS.AODP.Application.MediatR.Base;

public abstract class BaseResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    //public IEnumerable<BaseError>? Errors { get; set; }
}