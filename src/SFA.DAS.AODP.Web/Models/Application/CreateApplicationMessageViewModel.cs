using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Application;

public class CreateApplicationMessageViewModel
{
    public Guid OrganisationId { get; set; }
    public Guid ApplicationId { get; set; }
    [Required]
    public string MessageText { get; set; }
    [Required]
    public string MessageType { get; set; }


    public MessageActions AdditionalActions { get; set; } = new();

    public class MessageActions
    {
        public bool Preview { get; set; }
        public bool Send { get; set; }
    }
}
