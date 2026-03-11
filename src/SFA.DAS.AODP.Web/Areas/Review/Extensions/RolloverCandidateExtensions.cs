using Azure;
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
                    Name = g.First().FundingOffer!
                })
                .ToList();
        }

        public static List<QualificationCandidate> FilterCandidates(List<QualificationCandidate> items, IEnumerable<RolloverCandidate> rolloverCandidates)
        {
            var list = new List<QualificationCandidate>();

            foreach (var item in items)
            {
                var candidate = rolloverCandidates.Where(x => x.Qan == item.QualificationNumber).FirstOrDefault();

                if (candidate != null)
                    list.Add(new QualificationCandidate
                    {
                        QualificationNumber = candidate.Qan,
                        Title = candidate.Title,
                        FundingOfferId = candidate.FundingOfferId.ToString(),
                        FundingOffer = candidate.FundingOffer,
                        AwardingOrganisation = candidate.AwardingOrganisation,
                        FundingApprovalEndDate = candidate.FundingApprovalEndDate
                    });
            }

            return list;
        }
    }
}