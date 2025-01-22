namespace SFA.DAS.AODP.Domain.Interfaces;

public interface IInternalApiConfiguration : IApiConfiguration
{
    string Identifier { get; set; }
}