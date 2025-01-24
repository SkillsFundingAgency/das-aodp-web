using System.ComponentModel;

namespace SFA.DAS.AODP.Models.Forms.FormBuilder;

public enum FormStatus
{
    [Description("Draft")]
    Draft = 0,
    [Description("Published")]
    Published = 1,
    [Description("Archived")]
    Archived = 2
}