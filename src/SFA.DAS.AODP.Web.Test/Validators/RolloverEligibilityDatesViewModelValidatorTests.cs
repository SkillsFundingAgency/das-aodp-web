using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;
using SFA.DAS.AODP.Web.Validators;

namespace SFA.DAS.AODP.Web.UnitTests.Validators
{
    public class RolloverEligibilityDatesViewModelValidatorTests
    {
        private readonly RolloverEligibilityDatesViewModelValidator _sut = new();

        private static RolloverEligibilityDate Date(int? d, int? m, int? y)
            => new() { Day = d, Month = m, Year = y };

        [Fact]
        public void Validate_FundingEndDate_Null_Should_Have_Error()
        {
            var model = new RolloverEligibilityDatesViewModel
            {
                FundingEndDate = null,
                OperationalEndDate = Date(1, 1, 2030)
            };

            var result = _sut.Validate(model);

            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Funding end date is required");
            });
        }

        [Fact]
        public void Validate_OperationalEndDate_Null_Should_Have_Error()
        {
            var model = new RolloverEligibilityDatesViewModel
            {
                FundingEndDate = Date(1, 1, 2030),
                OperationalEndDate = null
            };

            var result = _sut.Validate(model);

            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Operational end date is required");
            });
        }

        [Fact]
        public void Validate_AllPartsMissing_Should_Have_Grouped_Errors()
        {
            var model = new RolloverEligibilityDatesViewModel
            {
                FundingEndDate = new(),
                OperationalEndDate = Date(1, 1, 2030)
            };

            var result = _sut.Validate(model);

            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Enter Funding end date");
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Enter a day");
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Enter a month");
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Enter a year");
            });
        }


        [Fact]
        public void Validate_DayMissing_Should_Have_PartialError()
        {
            var model = new RolloverEligibilityDatesViewModel
            {
                FundingEndDate = Date(null, 5, 2030),
                OperationalEndDate = Date(1, 1, 2030)
            };

            var result = _sut.Validate(model);

            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Funding end date must include a day");
                Assert.Contains(result.Errors, e => e.PropertyName == "FundingEndDate.Day" &&
                                                   e.ErrorMessage == "Enter a day");
            });
        }

        [Fact]
        public void Validate_MonthMissing_Should_Have_PartialError()
        {
            var model = new RolloverEligibilityDatesViewModel
            {
                FundingEndDate = Date(10, null, 2030),
                OperationalEndDate = Date(1, 1, 2030)
            };

            var result = _sut.Validate(model);

            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Funding end date must include a month");
                Assert.Contains(result.Errors, e => e.PropertyName == "FundingEndDate.Month" &&
                                                   e.ErrorMessage == "Enter a month");
            });
        }

        [Fact]
        public void Validate_YearMissing_Should_Have_PartialError()
        {
            var model = new RolloverEligibilityDatesViewModel
            {
                FundingEndDate = Date(10, 10, null),
                OperationalEndDate = Date(1, 1, 2030)
            };

            var result = _sut.Validate(model);

            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Funding end date must include a year");
                Assert.Contains(result.Errors, e => e.PropertyName == "FundingEndDate.Year" &&
                                                   e.ErrorMessage == "Enter a year");
            });
        }

        [Theory]
        [InlineData(0)]
        [InlineData(32)]
        public void Validate_InvalidDay_Should_Have_Error(int day)
        {
            var model = new RolloverEligibilityDatesViewModel
            {
                FundingEndDate = Date(day, 5, 2030),
                OperationalEndDate = Date(1, 1, 2030)
            };

            var result = _sut.Validate(model);

            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.PropertyName == "FundingEndDate.Day" &&
                                                   e.ErrorMessage == "Day must be between 1 and 31");
            });
        }

        [Theory]
        [InlineData(0)]
        [InlineData(13)]
        public void Validate_InvalidMonth_Should_Have_Error(int month)
        {
            var model = new RolloverEligibilityDatesViewModel
            {
                FundingEndDate = Date(10, month, 2030),
                OperationalEndDate = Date(1, 1, 2030)
            };

            var result = _sut.Validate(model);

            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.PropertyName == "FundingEndDate.Month" &&
                                                   e.ErrorMessage == "Month must be between 1 and 12");
            });
        }

        [Theory]
        [InlineData(999)]
        [InlineData(10000)]
        public void Validate_InvalidYear_Should_Have_Error(int year)
        {
            var model = new RolloverEligibilityDatesViewModel
            {
                FundingEndDate = Date(10, 10, year),
                OperationalEndDate = Date(1, 1, 2030)
            };

            var result = _sut.Validate(model);

            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.PropertyName == "FundingEndDate.Year" &&
                                                   e.ErrorMessage == "Year must be a 4-digit number");
            });
        }

        [Fact]
        public void Validate_InvalidRealDate_Should_Have_Error()
        {
            var model = new RolloverEligibilityDatesViewModel
            {
                FundingEndDate = Date(31, 2, 2030),
                OperationalEndDate = Date(1, 1, 2030)
            };

            var result = _sut.Validate(model);

            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);

                Assert.Contains(result.Errors, e => e.PropertyName == "FundingEndDate" &&
                                                   e.ErrorMessage == "Funding end date must be a real date");

                Assert.Contains(result.Errors, e => e.PropertyName == "FundingEndDate.Day" &&
                                                   e.ErrorMessage == "Enter a valid day");
                Assert.Contains(result.Errors, e => e.PropertyName == "FundingEndDate.Month" &&
                                                   e.ErrorMessage == "Enter a valid month");
                Assert.Contains(result.Errors, e => e.PropertyName == "FundingEndDate.Year" &&
                                                   e.ErrorMessage == "Enter a valid year");
            });
        }

        [Fact]
        public void Validate_DateInPast_Should_Have_Error()
        {
            var pastDate = DateTime.UtcNow.Date.AddDays(-1);

            var model = new RolloverEligibilityDatesViewModel
            {
                FundingEndDate = Date(pastDate.Day, pastDate.Month, pastDate.Year),
                OperationalEndDate = Date(1, 1, 2030)
            };

            var result = _sut.Validate(model);

            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Funding end date must be today or in the future");
            });
        }

        [Fact]
        public void Validate_ValidFutureDate_Should_Pass()
        {
            var futureDate = DateTime.UtcNow.Date.AddMonths(1);

            var model = new RolloverEligibilityDatesViewModel
            {
                FundingEndDate = Date(futureDate.Day, futureDate.Month, futureDate.Year),
                OperationalEndDate = Date(futureDate.Day, futureDate.Month, futureDate.Year)
            };

            var result = _sut.Validate(model);

            Assert.Multiple(() =>
            {
                Assert.True(result.IsValid);
                Assert.Empty(result.Errors);
            });
        }
    }
}