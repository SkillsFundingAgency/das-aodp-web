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
        public List<ProcessStatus> ProcessStatuses { get; set; } = new List<ProcessStatus>();

        public class ProcessStatus
        {
            public Guid Id { get; set; }
            public string? Name { get; set; }
            public int? IsOutcomeDecision { get; set; }
            public static implicit operator ProcessStatus(GetProcessStatusesQueryResponse.ProcessStatus v)
            {
                return new ProcessStatus
                {
                    Id = v.Id,
                    Name = v.Name,
                    IsOutcomeDecision = v.IsOutcomeDecision,
                };
            }
        }

        public static NewQualificationsViewModel Map(GetNewQualificationsQueryResponse response, List<GetProcessStatusesQueryResponse.ProcessStatus> processStatuses, string organisation = "", string qan = "", string name = "")
        {
            var viewModel = new NewQualificationsViewModel();
            viewModel.PaginationViewModel = new PaginationViewModel(response.TotalRecords, response.Skip, response.Take);
            viewModel.NewQualifications = response.Data.Select(s => new NewQualificationViewModel()
            {
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
            viewModel.ProcessStatuses = processStatuses.Select(v => new ProcessStatus()
            {
                Id = v.Id,
                Name = v.Name,
                IsOutcomeDecision = v.IsOutcomeDecision,
            }).ToList();

            return viewModel;
        }
    }
}
