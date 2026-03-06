using SFA.DAS.AODP.Application.Commands.Qualifications;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Models.BulkActions
{
    [ExcludeFromCodeCoverage]
    public class QualificationBulkActionResultViewModel
    {
        public string ProcessStatusName { get; set; } = "";
        public int RequestedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int ErrorCount { get; set; }

        public List<BulkStatusUpdateErrorItemViewModel> MissingQualifications { get; set; } = new();
        public List<BulkStatusUpdateErrorItemViewModel> StatusUpdateFailed { get; set; } = new();
        public List<BulkStatusUpdateErrorItemViewModel> HistoryUpdateFailed { get; set; } = new();

        public bool HasAnyErrors => ErrorCount > 0;

        public static QualificationBulkActionResultViewModel From(BulkUpdateQualificationStatusCommandResponse response)
        {
            var vm = new QualificationBulkActionResultViewModel
            {
                ProcessStatusName = response.ProcessStatusName ?? "",
                RequestedCount = response.RequestedCount,
                UpdatedCount = response.UpdatedCount,
                ErrorCount = response.ErrorCount
            };

            if (response.Errors == null) return vm;

            foreach (var e in response.Errors)
            {
                var item = new BulkStatusUpdateErrorItemViewModel
                {
                    QualificationId = e.QualificationId,
                    Qan = e.Qan ?? "",
                    Title = e.Title ?? "",
                    ErrorType = e.ErrorType
                };

                switch (e.ErrorType)
                {
                    case BulkQualificationErrorType.Missing:
                        vm.MissingQualifications.Add(item);
                        break;
                    case BulkQualificationErrorType.StatusUpdateFailed:
                        vm.StatusUpdateFailed.Add(item);
                        break;
                    case BulkQualificationErrorType.HistoryFailed:
                        vm.HistoryUpdateFailed.Add(item);
                        break;
                }
            }

            return vm;
        }
    }

    public class BulkStatusUpdateErrorItemViewModel
    {
        public Guid QualificationId { get; set; }
        public string Qan { get; set; } = "";
        public string Title { get; set; } = "";
        public BulkQualificationErrorType ErrorType { get; set; }
    }

}
