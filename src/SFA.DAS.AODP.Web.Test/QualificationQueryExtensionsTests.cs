using System;
using System.Collections.Generic;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Extensions;
using SFA.DAS.AODP.Web.Models.Qualifications;
using Xunit;

namespace SFA.DAS.AODP.Web.Tests.Extensions
{
    public class QualificationQueryExtensionsTests
    {
        private const int PageNumber = 3;
        private const int RecordsPerPage = 25;

        private const string QualificationName = "Diploma";
        private const string OrganisationName = "City & Guilds";
        private const string Qan = "12345678";

        private const string Whitespace = " ";

        [Fact]
        public void ToGetNewQualificationsQuery_Maps_Base_Properties()
        {
            var query = new QualificationQuery
            {
                PageNumber = PageNumber,
                RecordsPerPage = RecordsPerPage
            };

            var result = query.ToGetNewQualificationsQuery();

            Assert.Multiple(() =>
            {
                Assert.Equal(RecordsPerPage, result.Take);
                Assert.Equal(RecordsPerPage * (PageNumber - 1), result.Skip);
                Assert.Null(result.Name);
                Assert.Null(result.Organisation);
                Assert.Null(result.QAN);
                Assert.NotNull(result.ProcessStatusFilter);
                Assert.NotNull(result.ProcessStatusFilter.ProcessStatusIds);
                Assert.Empty(result.ProcessStatusFilter.ProcessStatusIds);
            });
        }

        [Fact]
        public void ToGetNewQualificationsQuery_Maps_Filter_Properties_When_Populated()
        {
            var processStatusIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            var query = new QualificationQuery
            {
                PageNumber = 2,
                RecordsPerPage = 10,
                Name = QualificationName,
                Organisation = OrganisationName,
                Qan = Qan,
                ProcessStatusIds = processStatusIds
            };

            var result = query.ToGetNewQualificationsQuery();

            Assert.Multiple(() =>
            {
                Assert.Equal(10, result.Take);
                Assert.Equal(10, result.Skip);
                Assert.Equal(QualificationName, result.Name);
                Assert.Equal(OrganisationName, result.Organisation);
                Assert.Equal(Qan, result.QAN);
                Assert.NotNull(result.ProcessStatusFilter);
                Assert.Equal(processStatusIds, result.ProcessStatusFilter.ProcessStatusIds);
            });
        }

        [Fact]
        public void ToGetNewQualificationsQuery_Does_Not_Map_Whitespace_Strings()
        {
            var query = new QualificationQuery
            {
                PageNumber = 1,
                RecordsPerPage = 20,
                Name = Whitespace,
                Organisation = Whitespace,
                Qan = ""
            };

            var result = query.ToGetNewQualificationsQuery();

            Assert.Multiple(() =>
            {
                Assert.Null(result.Name);
                Assert.Null(result.Organisation);
                Assert.Null(result.QAN);
            });
        }

        [Fact]
        public void ToQualificationFilterViewModel_Maps_Properties()
        {
            var processStatusIds = new List<Guid> { Guid.NewGuid() };

            var query = new QualificationQuery
            {
                Name = QualificationName,
                Organisation = OrganisationName,
                Qan = Qan,
                ProcessStatusIds = processStatusIds
            };

            var result = query.ToQualificationFilterViewModel();

            Assert.Multiple(() =>
            {
                Assert.Equal(OrganisationName, result.Organisation);
                Assert.Equal(QualificationName, result.QualificationName);
                Assert.Equal(Qan, result.QAN);
                Assert.Equal(processStatusIds, result.ProcessStatusIds);
            });
        }

        [Fact]
        public void ToQualificationFilterViewModel_Uses_Empty_String_For_Null_Strings()
        {
            var query = new QualificationQuery
            {
                Name = null,
                Organisation = null,
                Qan = null
            };

            var result = query.ToQualificationFilterViewModel();

            Assert.Multiple(() =>
            {
                Assert.Equal(string.Empty, result.Organisation);
                Assert.Equal(string.Empty, result.QualificationName);
                Assert.Equal(string.Empty, result.QAN);
                Assert.Null(result.ProcessStatusIds);
            });
        }

        [Fact]
        public void ToGetChangedQualificationsQuery_Maps_Base_And_Filter_Properties()
        {
            var processStatusIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            var query = new QualificationQuery
            {
                PageNumber = 4,
                RecordsPerPage = 5,
                Name = QualificationName,
                Organisation = OrganisationName,
                Qan = Qan,
                ProcessStatusIds = processStatusIds
            };

            var result = query.ToGetChangedQualificationsQuery();

            Assert.Multiple(() =>
            {
                Assert.Equal(5, result.Take);
                Assert.Equal(15, result.Skip);
                Assert.Equal(QualificationName, result.Name);
                Assert.Equal(OrganisationName, result.Organisation);
                Assert.Equal(Qan, result.QAN);
                Assert.Equal(processStatusIds, result.ProcessStatusIds);
            });
        }
    }
}