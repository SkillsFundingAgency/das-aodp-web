﻿using SFA.DAS.AODP.Application.Queries.Qualifications;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SFA.DAS.AODP.Web.Models.Qualifications;

public class NewQualificationDetailsTimelineViewModel
{
    public List<QualificationDiscussionHistory> QualificationDiscussionHistories { get; set; } = new List<QualificationDiscussionHistory>();
    public string Qan { get; set; } = string.Empty;
    public AdditionalFormActions AdditionalActions { get; set; } = new AdditionalFormActions();

    public class AdditionalFormActions
    {
        public string Note { get; set; } = string.Empty;
        public bool AddNote { get; set; }
    }
    public static implicit operator NewQualificationDetailsTimelineViewModel(GetDiscussionHistoriesForQualificationQueryResponse model)
    {
        return new NewQualificationDetailsTimelineViewModel()
        {
            QualificationDiscussionHistories = [..model.QualificationDiscussionHistories]
        };
    }
    public partial class QualificationDiscussionHistory
    {
        public Guid Id { get; set; }
        public Guid QualificationId { get; set; }
        public Guid ActionTypeId { get; set; }
        public string? UserDisplayName { get; set; }
        public string? Notes { get; set; }
        public DateTime? Timestamp { get; set; }
        public virtual ActionType ActionType { get; set; } = null!;
        public string FormattedTimestamp
        {
            get => Timestamp is null ? "" :
                Timestamp.Value.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) 
                    + " at " 
                    + Timestamp.Value.ToString("HH:mm", CultureInfo.InvariantCulture);
        }

        public static implicit operator QualificationDiscussionHistory(GetDiscussionHistoriesForQualificationQueryResponse.QualificationDiscussionHistory model)
        {
            return new QualificationDiscussionHistory
            {
                Id = model.Id,
                QualificationId = model.QualificationId,
                ActionTypeId = model.ActionTypeId,
                UserDisplayName = model.UserDisplayName,
                Notes = model.Notes,
                Timestamp = model.Timestamp,
                ActionType = new ActionType
                {
                    Id = model.ActionType.Id,
                    Description = model.ActionType.Description,
                }
            };
        }
    }
    public class ActionType
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
    }
}

