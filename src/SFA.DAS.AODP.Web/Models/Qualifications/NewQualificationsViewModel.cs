using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Domain.Qualifications.Requests;
using SFA.DAS.AODP.Models.Qualifications;
using SFA.DAS.AODP.Web.Extensions;
using SFA.DAS.AODP.Web.Models.BulkActions;

namespace SFA.DAS.AODP.Web.Models.Qualifications
{
    public class NewQualificationsViewModel : QualificationsBulkActionPageViewModel
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

        public IEnumerable<(AgeGroup Value, string Label)> AvailableAgeGroups =>
            Enum.GetValues<AgeGroup>()
                .Select(x => (x, x switch
                {
                    AgeGroup.Pre16 => "Pre-16",
                    AgeGroup.SixteenToEighteen => "16 to 18",
                    AgeGroup.EighteenPlus => "18+",
                    AgeGroup.NineteenPlus => "19+",
                    _ => x.ToString()
                }));
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

        public static NewQualificationsViewModel Map(
            GetNewQualificationsQueryResponse response, 
            List<GetProcessStatusesQueryResponse.ProcessStatus> processStatuses,
            QualificationQuery qualificationQuery)
        {
            var viewModel = new NewQualificationsViewModel();
            viewModel.PaginationViewModel = new PaginationViewModel(response.TotalRecords, response.Skip, response.Take);
            viewModel.NewQualifications = response.Data.Select(s => new NewQualificationViewModel()
            {
                QualificationId = s.QualificationId,
                AwardingOrganisation = s.AwardingOrganisation,
                Reference = s.Reference,
                Status = s.Status,
                Title = s.Title,                
                AgeGroup = s.AgeGroup,
                EligibilityStatus = (s.EligibleForFunding ?? false) ? "Eligible" : "Not eligible"
            }).ToList();
            viewModel.Filter = qualificationQuery.ToQualificationFilterViewModel();
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
