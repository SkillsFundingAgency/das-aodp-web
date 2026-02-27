using Microsoft.AspNetCore.Components.Forms;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Models.BulkActions;
using SFA.DAS.AODP.Web.Validators.Attributes;
using SFA.DAS.AODP.Web.Validators.Messages;
using SFA.DAS.AODP.Web.Validators.Patterns;
using System.ComponentModel.DataAnnotations;

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

        public string FindRegulatedQualificationUrl { get; set; } = string.Empty;

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

        public class BulkActionViewModel
        {
            [Display(Name ="Status")]
            [Required(ErrorMessage = ValidationMessages.QualificationsBulkActionStatusRequired)]
            public Guid? ProcessStatusId { get; set; }

            [AllowedCharactersAttribute(TextCharacterProfile.FreeText)]
            public string? Comment { get; set; }
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
                AgeGroup = s.AgeGroup
            }).ToList();
            viewModel.Filter = new NewQualificationFilterViewModel()
            {
                 Organisation = qualificationQuery.Organisation,
                 QAN = qualificationQuery.Qan,
                 QualificationName = qualificationQuery.Name
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
