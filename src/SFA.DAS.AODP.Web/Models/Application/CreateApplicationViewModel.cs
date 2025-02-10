﻿using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Application
{
    public class CreateApplicationViewModel
    {
        public Guid OrganisationId { get; set; }
        public Guid FormVersionId { get; set; }

        public string FormTitle { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string Owner { get; set; }

    }
}