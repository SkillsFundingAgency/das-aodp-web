using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.AODP.Models.Qualification;

namespace SFA.DAS.AODP.Infrastructure.Context
{
    public interface IApplicationDbContext
    {
        DbSet<ApprovedQualification> ApprovedQualifications { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    }
}
