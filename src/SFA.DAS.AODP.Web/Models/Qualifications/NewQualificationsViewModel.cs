using SFA.DAS.AODP.Application.Queries.Qualifications;

namespace SFA.DAS.AODP.Web.Models.Qualifications
{
    public class NewQualificationsViewModel
    {
        public NewQualificationsViewModel()
        {
            NewQualifications = new List<NewQualificationViewModel>();
            JobStatusViewModel = new JobStatusViewModel();
            Filter = new NewQualificationFilterViewModel();
            PaginationViewModel = new PaginationViewModel();
        }

        public NewQualificationFilterViewModel Filter { get; set; }

        public List<NewQualificationViewModel> NewQualifications { get; set; }

        public PaginationViewModel PaginationViewModel { get; set; }

        public JobStatusViewModel JobStatusViewModel { get; set; }

        public static NewQualificationsViewModel Map(GetNewQualificationsQueryResponse response, string organisation = "", string qan = "", string name = "")
        {
            var viewModel = new NewQualificationsViewModel();
            viewModel.PaginationViewModel = new PaginationViewModel(response.TotalRecords, response.Skip, response.Take);
            viewModel.NewQualifications = response.Data.Select(s => new NewQualificationViewModel()
            {
                Id = s.Id,
                AwardingOrganisation = s.AwardingOrganisation,
                Reference = s.Reference,
                Status = s.Status,
                Title = s.Title,                
                AgeGroup = s.AgeGroup
            }).ToList();
            viewModel.Filter = new NewQualificationFilterViewModel()
            {
                 Organisation = organisation,
                 QAN = qan,
                 QualificationName = name
            };
            viewModel.JobStatusViewModel = new JobStatusViewModel()
            {
                Name = response.Job.Name,
                LastRunTime = response.Job.LastRunTime,
                Status = response.Job.Status
            };

            return viewModel;
        }
    }
}
