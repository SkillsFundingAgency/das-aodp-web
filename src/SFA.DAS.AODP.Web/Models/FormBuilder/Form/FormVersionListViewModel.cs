﻿using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
using SFA.DAS.AODP.Models.Forms;

namespace SFA.DAS.AODP.Web.Models.FormBuilder.Form
{
    public class FormVersionListViewModel
    {
        public List<FormVersion> FormVersions { get; set; } = new();
        public FormActions AdditionalActions { get; set; } = new();

        public class FormActions
        {
            public Guid? CreateDraft { get; set; }
        }

        public class FormVersion
        {
            public string? Title { get; set; }
            public Guid? DraftVersionId { get; set; }
            public Guid? PublishedVersionId { get; set; }
            public string? Status { get; set; }
            public int? Order { get; set; }
            public Guid? FormId { get; set; }

        }

        public static FormVersionListViewModel Map(GetAllFormVersionsQueryResponse response)
        {
            var viewModel = new FormVersionListViewModel();
            if (response?.Data == null || response?.Data?.Any() == false) return viewModel;

            var group = response.Data.GroupBy(x => x.FormId);

            foreach (var item in group)
            {
                var published = item.FirstOrDefault(g => g.Status == FormStatus.Published.ToString());
                var draft = item.FirstOrDefault(g => g.Status == FormStatus.Draft.ToString());

                var dataItem = new FormVersion
                {
                    DraftVersionId = draft?.Id,
                    PublishedVersionId = published?.Id,
                    Title = draft?.Title ?? published?.Title,
                    Status = published != null ? "Published" : "Draft",
                    Order = draft?.Order ?? published?.Order,
                    FormId = draft?.FormId ?? published?.FormId,
                };

                viewModel.FormVersions.Add(dataItem);
            }

            return viewModel;
        }
    }
}
