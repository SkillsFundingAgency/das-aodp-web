using MediatR;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Models.Qualifications;

namespace SFA.DAS.AODP.Application.Queries.Test
{
    public class GetNewQualificationsQueryHandler : IRequestHandler<GetNewQualificationsQuery, GetNewQualificationsQueryResponse>
    {
        public Task<GetNewQualificationsQueryResponse> Handle(GetNewQualificationsQuery request, CancellationToken cancellationToken)
        {
            // Stubbed data until API is available
            var mockQualifications = new List<NewQualification>
            {
                new NewQualification { Id = 1, Title = "EDEXCEL Intermediate GNVQ in Retail and Distributive Services" },
                new NewQualification { Id = 2, Title = "EDEXCEL Intermediate GNVQ in Art and Design" },
                new NewQualification { Id = 3, Title = "EDEXCEL Intermediate GNVQ in Business" },
                new NewQualification { Id = 4, Title = "OCR Intermediate GNVQ in Science" }
            };

            return Task.FromResult(new GetNewQualificationsQueryResponse
            {
                Success = true,
                NewQualifications = mockQualifications
            });
        }
    }
}
