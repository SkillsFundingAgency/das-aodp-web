using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Import
{
    public class ImportViewModel
    {
		[Required]
		public string Title { get; set; } = string.Empty;

		[Required]
        public bool Required { get; set; }
    }
}
