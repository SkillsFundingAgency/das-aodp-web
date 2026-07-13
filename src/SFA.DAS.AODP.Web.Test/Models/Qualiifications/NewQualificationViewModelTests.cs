using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Domain.Qualifications.Requests;
using SFA.DAS.AODP.Web.Models.Qualifications;

namespace SFA.DAS.AODP.Web.UnitTests.Models.Qualiifications
{
    public class NewQualificationViewModelTests
    {
        [Fact]
        public void AvailableAgeGroups_Should_Return_Expected_Labels()
        {
            // Arrange
            var viewModel = new NewQualificationsViewModel(); 

            // Act
            var result = viewModel.AvailableAgeGroups.ToList();

            // Assert
            Assert.Contains(result, x => x.Value == AgeGroup.Pre16 && x.Label == "Pre-16");
            Assert.Contains(result, x => x.Value == AgeGroup.SixteenToEighteen && x.Label == "16 to 18");
            Assert.Contains(result, x => x.Value == AgeGroup.EighteenPlus && x.Label == "18+");
            Assert.Contains(result, x => x.Value == AgeGroup.NineteenPlus && x.Label == "19+");
        }
    }
}
