using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using SFA.DAS.AODP.Authentication.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.AODP.Authentication.Services
{
    public class StubSchemeProvider : AuthenticationSchemeProvider
    {
        public StubSchemeProvider(IOptions<AuthenticationOptions> options)
            : base(options)
        {
        }

        protected StubSchemeProvider(
            IOptions<AuthenticationOptions> options,
            IDictionary<string, AuthenticationScheme> schemes
        )
            : base(options, schemes)
        {
        }

        public override Task<AuthenticationScheme> GetSchemeAsync(string name)
        {
            if (name == "SFA.DAS.AODP.Web")
            {
                var scheme = new AuthenticationScheme(
                    "SFA.DAS.AODP.Web",
                    "SFA.DAS.AODP.Web",
                    typeof(StubAuthHandler)
                );
                return Task.FromResult(scheme);
            }

            return base.GetSchemeAsync(name);
        }
    }
}
