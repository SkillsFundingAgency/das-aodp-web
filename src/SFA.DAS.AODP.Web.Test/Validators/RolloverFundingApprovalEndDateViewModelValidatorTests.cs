using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.UnitTests.Validators
{
    public class RolloverFundingApprovalEndDateViewModelValidatorTests
    {
        private readonly RolloverFundingApprovalEndDateViewModelValidator _sut = new();

        [Fact]
        public void Validate_MaxApprovalEndDate_Null_Should_Have_Error()
        {
            // Arrange
            var model = new RolloverFundingApprovalEndDateViewModel { MaxApprovalEndDate = null };

            // Act
            var result = _sut.Validate(model);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Max approval end date is required");
            });
        }

        [Fact]
        public void Validate_AllDateParts_Missing_Should_Have_GroupedError()
        {
            // Arrange
            var model = new RolloverFundingApprovalEndDateViewModel
            {
                MaxApprovalEndDate = new RolloverFundingApprovalEndDate
                {
                    Day = null,
                    Month = null,
                    Year = null
                }
            };

            // Act
            var result = _sut.Validate(model);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Enter Max approval end date");
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Enter a day");
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Enter a month");
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Enter a year");
            });
        }

        [Fact]
        public void Validate_Day_Missing_Should_Have_PartialError()
        {
            // Arrange
            var model = new RolloverFundingApprovalEndDateViewModel
            {
                MaxApprovalEndDate = new RolloverFundingApprovalEndDate
                {
                    Day = null,
                    Month = 6,
                    Year = 2025
                }
            };

            // Act
            var result = _sut.Validate(model);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Max approval end date must include a day");
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Enter a day");
            });
        }

        [Fact]
        public void Validate_Month_Missing_Should_Have_PartialError()
        {
            // Arrange
            var model = new RolloverFundingApprovalEndDateViewModel
            {
                MaxApprovalEndDate = new RolloverFundingApprovalEndDate
                {
                    Day = 15,
                    Month = null,
                    Year = 2025
                }
            };

            // Act
            var result = _sut.Validate(model);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Max approval end date must include a month");
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Enter a month");
            });
        }

        [Fact]
        public void Validate_Year_Missing_Should_Have_PartialError()
        {
            // Arrange
            var model = new RolloverFundingApprovalEndDateViewModel
            {
                MaxApprovalEndDate = new RolloverFundingApprovalEndDate
                {
                    Day = 15,
                    Month = 6,
                    Year = null
                }
            };

            // Act
            var result = _sut.Validate(model);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Max approval end date must include a year");
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Enter a year");
            });
        }

        [Fact]
        public void Validate_Day_And_Month_Missing_Should_Have_PartialError()
        {
            // Arrange
            var model = new RolloverFundingApprovalEndDateViewModel
            {
                MaxApprovalEndDate = new RolloverFundingApprovalEndDate
                {
                    Day = null,
                    Month = null,
                    Year = 2025
                }
            };

            // Act
            var result = _sut.Validate(model);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Max approval end date must include a day and month");
            });
        }

        [Fact]
        public void Validate_Day_OutOfRange_Below_Should_Have_Error()
        {
            // Arrange
            var model = new RolloverFundingApprovalEndDateViewModel
            {
                MaxApprovalEndDate = new RolloverFundingApprovalEndDate
                {
                    Day = 0,
                    Month = 6,
                    Year = 2025
                }
            };

            // Act
            var result = _sut.Validate(model);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Day must be between 1 and 31");
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Enter a valid day");
            });
        }

        [Fact]
        public void Validate_Day_OutOfRange_Above_Should_Have_Error()
        {
            // Arrange
            var model = new RolloverFundingApprovalEndDateViewModel
            {
                MaxApprovalEndDate = new RolloverFundingApprovalEndDate
                {
                    Day = 32,
                    Month = 6,
                    Year = 2025
                }
            };

            // Act
            var result = _sut.Validate(model);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Day must be between 1 and 31");
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Enter a valid day");
            });
        }

        [Fact]
        public void Validate_Month_OutOfRange_Below_Should_Have_Error()
        {
            // Arrange
            var model = new RolloverFundingApprovalEndDateViewModel
            {
                MaxApprovalEndDate = new RolloverFundingApprovalEndDate
                {
                    Day = 15,
                    Month = 0,
                    Year = 2025
                }
            };

            // Act
            var result = _sut.Validate(model);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Month must be between 1 and 12");
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Enter a valid month");
            });
        }

        [Fact]
        public void Validate_Month_OutOfRange_Above_Should_Have_Error()
        {
            // Arrange
            var model = new RolloverFundingApprovalEndDateViewModel
            {
                MaxApprovalEndDate = new RolloverFundingApprovalEndDate
                {
                    Day = 15,
                    Month = 13,
                    Year = 2025
                }
            };

            // Act
            var result = _sut.Validate(model);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Month must be between 1 and 12");
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Enter a valid month");
            });
        }

        [Fact]
        public void Validate_Year_OutOfRange_Below_Should_Have_Error()
        {
            // Arrange
            var model = new RolloverFundingApprovalEndDateViewModel
            {
                MaxApprovalEndDate = new RolloverFundingApprovalEndDate
                {
                    Day = 15,
                    Month = 6,
                    Year = 999
                }
            };

            // Act
            var result = _sut.Validate(model);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Year must be a 4-digit number");
            });
        }

        [Fact]
        public void Validate_Year_OutOfRange_Above_Should_Have_Error()
        {
            // Arrange
            var model = new RolloverFundingApprovalEndDateViewModel
            {
                MaxApprovalEndDate = new RolloverFundingApprovalEndDate
                {
                    Day = 15,
                    Month = 6,
                    Year = 10000
                }
            };

            // Act
            var result = _sut.Validate(model);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Year must be a 4-digit number");
            });
        }

        [Fact]
        public void Validate_InvalidDate_Should_Have_Error()
        {
            // Arrange
            var model = new RolloverFundingApprovalEndDateViewModel
            {
                MaxApprovalEndDate = new RolloverFundingApprovalEndDate
                {
                    Day = 31,
                    Month = 2,
                    Year = 2025
                }
            };

            // Act
            var result = _sut.Validate(model);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Max approval end date must be a real date");
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Enter a valid day");
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Enter a valid month");
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Enter a valid year");
            });
        }

        [Fact]
        public void Validate_DateInPast_Should_Have_Error()
        {
            // Arrange
            var model = new RolloverFundingApprovalEndDateViewModel
            {
                MaxApprovalEndDate = new RolloverFundingApprovalEndDate
                {
                    Day = 1,
                    Month = 1,
                    Year = 2020
                }
            };

            // Act
            var result = _sut.Validate(model);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Max approval end date must be in the future");
            });
        }

        [Fact]
        public void Validate_DateToday_Should_Have_Error()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            var model = new RolloverFundingApprovalEndDateViewModel
            {
                MaxApprovalEndDate = new RolloverFundingApprovalEndDate
                {
                    Day = today.Day,
                    Month = today.Month,
                    Year = today.Year
                }
            };

            // Act
            var result = _sut.Validate(model);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.ErrorMessage == "Max approval end date must be in the future");
            });
        }

        [Fact]
        public void Validate_ValidFutureDate_Should_Be_Valid()
        {
            // Arrange
            var futureDate = DateTime.UtcNow.Date.AddYears(1);
            var model = new RolloverFundingApprovalEndDateViewModel
            {
                MaxApprovalEndDate = new RolloverFundingApprovalEndDate
                {
                    Day = futureDate.Day,
                    Month = futureDate.Month,
                    Year = futureDate.Year
                }
            };

            // Act
            var result = _sut.Validate(model);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }
    }
}
