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

        public static ChangedQualificationsViewModel Map(GetChangedQualificationsQueryResponse response, string organisation = "", string qan = "", string name = "")
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
