using SFA.DAS.AODP.Application.Commands.Review;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Models.BulkActions
{
    [ExcludeFromCodeCoverage]
    public class ApplicationsBulkActionResultViewModel
    {
        public int RequestedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int ErrorCount { get; set; }

        public List<ApplicationsBulkActionErrorItemViewModel> List1 { get; set; } = new();
        public List<ApplicationsBulkActionErrorItemViewModel> List2 { get; set; } = new();
        public List<ApplicationsBulkActionErrorItemViewModel> List3 { get; set; } = new();

        public bool HasAnyErrors => ErrorCount > 0;

        public static ApplicationsBulkActionResultViewModel From(BulkApplicationActionCommandResponse response)
        {
            var vm = new ApplicationsBulkActionResultViewModel
            {
                //RequestedCount = response.,
                //UpdatedCount = response.UpdatedCount,
                ErrorCount = response.ErrorCount
            };

            //if (response. == null) return vm;

            //foreach (var e in response.Errors)
            //{
            //    var item = new ApplicationsBulkStatusUpdateErrorItemViewModel
            //    {
            //        //QualificationId = e.QualificationId,
            //        //Qan = e.Qan ?? "",
            //        //Title = e.Title ?? "",
            //        //ErrorType = e.ErrorType
            //    };

            //    switch (e.ErrorType)
            //    {
            //        case BulkUpdateQualificationsErrorType.Missing:
            //            vm.MissingQualifications.Add(item);
            //            break;
            //        case BulkUpdateQualificationsErrorType.StatusUpdateFailed:
            //            vm.StatusUpdateFailed.Add(item);
            //            break;
            //        case BulkUpdateQualificationsErrorType.HistoryFailed:
            //            vm.HistoryUpdateFailed.Add(item);
            //            break;
            //    }
            //}

            return vm;
        }

    }

    public class ApplicationsBulkActionErrorItemViewModel
    {
        //public Guid QualificationId { get; set; }
        //public string Qan { get; set; } = "";
        //public string Title { get; set; } = "";
        //public BulkQualificationErrorType ErrorType { get; set; }
    }

}
