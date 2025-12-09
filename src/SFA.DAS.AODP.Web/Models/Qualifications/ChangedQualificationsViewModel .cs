using SFA.DAS.AODP.Application.Queries.Qualifications;

namespace SFA.DAS.AODP.Web.Models.Qualifications
{
    public class ChangedQualificationsViewModel
    {
        public ChangedQualificationsViewModel()
        {
            ChangedQualifications = new List<ChangedQualificationViewModel>();
            JobStatusViewModel = new JobStatusViewModel();
            Filter = new NewQualificationFilterViewModel();
            PaginationViewModel = new PaginationViewModel();
        }

        public NewQualificationFilterViewModel Filter { get; set; }

        public List<ChangedQualificationViewModel> ChangedQualifications { get; set; }

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

        public static ChangedQualificationsViewModel Map(GetChangedQualificationsQueryResponse response, List<GetProcessStatusesQueryResponse.ProcessStatus> processStatuses,string organisation = "", string qan = "", string name = "")
        {
            var viewModel = new ChangedQualificationsViewModel();
            viewModel.PaginationViewModel = new PaginationViewModel(response.TotalRecords, response.Skip, response.Take);
            viewModel.ChangedQualifications= response.Data.Select(s => new ChangedQualificationViewModel()
            {
                QualificationReference=s.QualificationReference,
                AwardingOrganisation = s.AwardingOrganisation,
                QualificationTitle=s.QualificationTitle,
                QualificationType=s.QualificationType,
                Subject=s.Subject,
                Level=s.Level,
                SectorSubjectArea=s.SectorSubjectArea,
                AgeGroup = s.AgeGroup,
                ChangedFieldNames = s.ChangedFieldNames,
                Status=s.Status
                
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
