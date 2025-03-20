using SFA.DAS.AODP.Application.Queries.Qualifications;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SFA.DAS.AODP.Web.Models.Qualifications;

public class NewQualificationDetailsStatusViewModel
{
    public List<ProcessStatus> ProcessStatuses { get; set; } = new List<ProcessStatus>();
    public string Qan { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public Guid ProcessStatusId { get; set; }

    public static implicit operator NewQualificationDetailsStatusViewModel(GetProcessStatusesQueryResponse model)
    {
        return new NewQualificationDetailsStatusViewModel
        {
            ProcessStatuses = [.. model.ProcessStatuses]
        };
    }

    public class ProcessStatus
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public int? IsOutcomeDecision { get; set; }

        public static implicit operator ProcessStatus(GetProcessStatusesQueryResponse.ProcessStatus model)
        {
            return new ProcessStatus
            {
                Id = model.Id,
                Name = model.Name,
                IsOutcomeDecision = model.IsOutcomeDecision,
            };
        }
    }
}

