using SFA.DAS.AODP.Application.Queries.Review.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.Areas.Review.Extensions
{
    public static class RolloverCandidateExtensions
    {
        public static List<FundingStream> ToFundingStreams(List<QualificationCandidate> candidates)
        {
            return candidates
                .GroupBy(c => c.FundingOfferId)
                .Select(g => new FundingStream
                {
                    Id = g.First().FundingOfferId!,
                    Name = g.First().FundingOfferName!
                })
                .OrderBy(x => x.Name)
                .ToList();
        }

        public static List<QualificationCandidate> FilterCandidates(List<QualificationCandidate> items, IEnumerable<RolloverCandidate> rolloverCandidates)
        {
            var list = new List<QualificationCandidate>();

            foreach (var item in items)
            {
                var candidates = rolloverCandidates.Where(x => x.QualificationNumber == item.QualificationNumber).ToList();

                foreach (var candidate in candidates)
                {
                    list.Add(new QualificationCandidate
                    {
                        QualificationNumber = candidate.QualificationNumber,
                        FundingOfferId = candidate.FundingOfferId,
                        FundingOfferName = candidate.FundingOfferName,
                        AcademicYear = candidate.AcademicYear,
                        RolloverCandidateId = candidate.Id,
                        QualificationVersionId = candidate.QualificationVersionId,
                        AwardingOrganisation = item.AwardingOrganisation,
                        FundingApprovalEndDate = item.FundingApprovalEndDate,
                        QualificationName = item.QualificationName
                    });
                }
            }

            return list;
        }
    }
}