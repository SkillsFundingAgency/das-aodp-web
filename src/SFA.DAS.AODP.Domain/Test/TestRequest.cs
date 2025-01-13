using SFA.DAS.FAA.Domain.Interfaces;

namespace SFA.DAS.FAA.Domain.GetApprenticeshipVacancy
{
    public class TestRequest(string vacancyReference, string? candidateId) : IGetApiRequest
    {
        public string GetUrl => $"vacancies/{vacancyReference}?candidateId={candidateId}";
    }
}
