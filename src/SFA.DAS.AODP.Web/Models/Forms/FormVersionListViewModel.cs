using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Web.Models.Forms
{
    public class FormVersionListViewModel
    {
        public List<FormVersion> FormVersions { get; set; } = new();

        public class FormVersion
        {
            public string? Name { get; set; }
            public Guid? DraftVersionId { get; set; }
            public Guid? PublishedVersionId { get; set; }
            public string? Status { get; set; }
            public int? Order { get; set; }

        }

        public static FormVersionListViewModel Map(GetAllFormVersionsQueryResponse response)
        {
            var viewModel = new FormVersionListViewModel();
            response.Data.Add(new()
            {
                Status = FormStatus.Published,
                Name = "Published",
                Order = 1
            });

            var group = response.Data.GroupBy(x => x.FormId);

            foreach (var item in group)
            {
                var published = item.FirstOrDefault(g => g.Status == FormStatus.Published);
                var draft = item.FirstOrDefault(g => g.Status == FormStatus.Draft);

                var dataItem = new FormVersion
                {
                    DraftVersionId = draft?.Id,
                    PublishedVersionId = published?.Id,
                    Name = draft?.Name ?? published?.Name,
                    Status = published != null ? "Published" : "Draft",
                    Order = draft?.Order ?? published?.Order,
                };

                viewModel.FormVersions.Add(dataItem);
            }

            return viewModel;
        }
    }
}
