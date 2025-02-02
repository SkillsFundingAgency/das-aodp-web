using MediatR;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetQualificationDetailsHandler : IRequestHandler<GetQualificationDetailsQuery, GetQualificationDetailsQueryResponse>
    {
        public async Task<GetQualificationDetailsQueryResponse> Handle(GetQualificationDetailsQuery request, CancellationToken cancellationToken)
        {
            var qualificationDetails = MockQualificationDetails.Find(qd => qd.Id == request.Id);

            return await Task.FromResult(qualificationDetails ?? new GetQualificationDetailsQueryResponse { Success = false });
        }

        private static readonly List<GetQualificationDetailsQueryResponse> MockQualificationDetails = new()
        {
            new GetQualificationDetailsQueryResponse
            {
                Id = 1,
                Success = true,
                Status = "Decision required",
                Priority = "High",
                Changes = "Qualification title, Level",
                QualificationReference = "XXXXX001",
                AwardingOrganisation = "International Baccalaureate Organisation",
                Title = "IBO Level 3 Alternative Academic Qualification HL in Chemistry (Extended Certificate)",
                QualificationType = "Non-technical",
                Level = "3",
                ProposedChanges = "Proposed: 2",
                AgeGroup = "16 - 18",
                Category = "Alternative academic qualification",
                Subject = "Science and Mathematics",
                SectorSubjectArea = "Science",
                Comments = "Comments here"
            }
        };

    }
}
