using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;


namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers
{
    public class FundingStreamInclusionExclusionTests : RolloverControllerTestBase
    {
        // -------------------------------------------------------
        // GET
        // -------------------------------------------------------

        [Fact]
        public async Task FundingStreamInclusionExclusion_Get_WhenSessionHasFundingStream_PopulatesModel()
        {
            var session = CreateEmptySession();

            var fs1 = new FundingStream { Id = Guid.NewGuid(), Name = "FS1" };

            var saved = new Rollover
            {
                RolloverFundingStream = new RolloverFundingStream
                {
                    FundingStreams = new List<FundingStream> { fs1 },
                    SelectedIds = new List<Guid> { fs1.Id }
                }
            };

            session.SetString("RolloverSession", JsonConvert.SerializeObject(saved));

            var controller = CreateController(session);

            var result = await controller.FundingStreamInclusionExclusion();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<FundingStreamInclusionExclusionViewModel>(view.Model);

            Assert.Single(model.FundingStreams);
            Assert.Single(model.SelectedIds);
        }

        [Fact]
        public async Task FundingStreamInclusionExclusion_Get_WhenNoFundingStreamsFound_AddsModelError()
        {
            var session = CreateEmptySession();

            var saved = new Rollover
            {
                RolloverCandidates = new List<QualificationCandidate>() // no funding streams
            };

            session.SetString("RolloverSession", JsonConvert.SerializeObject(saved));

            var controller = CreateController(session);

            var result = await controller.FundingStreamInclusionExclusion();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<FundingStreamInclusionExclusionViewModel>(view.Model);

            Assert.True(controller.ModelState.ContainsKey(nameof(model.FundingStreams)));
        }

        // -------------------------------------------------------
        // POST
        // -------------------------------------------------------

        [Fact]
        public async Task FundingStreamInclusionExclusion_Post_SelectAll_ReturnsViewWithAllIdsSelected()
        {
            var session = CreateEmptySession();

            var fs1 = new FundingStream { Id = Guid.NewGuid(), Name = "FS1" };
            var fs2 = new FundingStream { Id = Guid.NewGuid(), Name = "FS2" };

            var saved = new Rollover
            {
                RolloverFundingStream = new RolloverFundingStream
                {
                    FundingStreams = new List<FundingStream> { fs1, fs2 },
                    SelectedIds = new List<Guid>()
                }
            };

            session.SetString("RolloverSession", JsonConvert.SerializeObject(saved));

            var controller = CreateController(session);

            var vm = new FundingStreamInclusionExclusionViewModel
            {
                FundingStreams = new List<FundingStream> { fs1, fs2 }
            };

            var result = await controller.FundingStreamInclusionExclusion(vm, "selectAll");

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<FundingStreamInclusionExclusionViewModel>(view.Model);

            Assert.Equal(2, model.SelectedIds.Count);
        }

        [Fact]
        public async Task FundingStreamInclusionExclusion_Post_NoSelection_ReturnsViewWithError()
        {
            var session = CreateEmptySession();

            var fs = new FundingStream { Id = Guid.NewGuid(), Name = "FS1" };

            var saved = new Rollover
            {
                RolloverFundingStream = new RolloverFundingStream
                {
                    FundingStreams = new List<FundingStream> { fs }
                }
            };

            session.SetString("RolloverSession", JsonConvert.SerializeObject(saved));

            var controller = CreateController(session);

            var vm = new FundingStreamInclusionExclusionViewModel
            {
                FundingStreams = new List<FundingStream> { fs },
                SelectedIds = new()
            };

            var result = await controller.FundingStreamInclusionExclusion(vm, action: "");

            var view = Assert.IsType<ViewResult>(result);
            Assert.True(controller.ModelState.ContainsKey(nameof(vm.SelectedIds)));
        }

        [Fact]
        public async Task FundingStreamInclusionExclusion_Post_EmptySelection_ReturnsViewWithError()
        {
            var session = CreateEmptySession();

            var fs = new FundingStream { Id = Guid.NewGuid(), Name = "FS1" };

            var saved = new Rollover
            {
                RolloverFundingStream = new RolloverFundingStream
                {
                    FundingStreams = new List<FundingStream> { fs }
                }
            };

            session.SetString("RolloverSession", JsonConvert.SerializeObject(saved));

            var controller = CreateController(session);

            var vm = new FundingStreamInclusionExclusionViewModel
            {
                FundingStreams = new List<FundingStream> { fs },
                SelectedIds = new List<Guid>() // empty
            };

            var result = await controller.FundingStreamInclusionExclusion(vm, action: "");

            var view = Assert.IsType<ViewResult>(result);
            Assert.True(controller.ModelState.ContainsKey(nameof(vm.SelectedIds)));
        }

        [Fact]
        public async Task FundingStreamInclusionExclusion_Post_InvalidSelection_ReturnsViewWithError()
        {
            var session = CreateEmptySession();

            var fs = new FundingStream { Id = Guid.NewGuid(), Name = "FS1" };

            var saved = new Rollover
            {
                RolloverFundingStream = new RolloverFundingStream
                {
                    FundingStreams = new List<FundingStream> { fs }
                }
            };

            session.SetString("RolloverSession", JsonConvert.SerializeObject(saved));

            var controller = CreateController(session);

            var vm = new FundingStreamInclusionExclusionViewModel
            {
                FundingStreams = new List<FundingStream> { fs },
                SelectedIds = new List<Guid> { Guid.NewGuid() } // invalid ID
            };

            var result = await controller.FundingStreamInclusionExclusion(vm, action: "");

            var view = Assert.IsType<ViewResult>(result);
            Assert.True(controller.ModelState.ContainsKey(string.Empty));
        }

        [Fact]
        public async Task FundingStreamInclusionExclusion_Post_ValidSelection_SavesSessionAndRedirects()
        {
            var session = CreateEmptySession();

            var fs = new FundingStream { Id = Guid.NewGuid(), Name = "FS1" };

            var saved = new Rollover
            {
                RolloverFundingStream = new RolloverFundingStream
                {
                    FundingStreams = new List<FundingStream> { fs }
                }
            };

            session.SetString("RolloverSession", JsonConvert.SerializeObject(saved));

            var controller = CreateController(session);

            var vm = new FundingStreamInclusionExclusionViewModel
            {
                FundingStreams = new List<FundingStream> { fs },
                SelectedIds = new List<Guid> { fs.Id }
            };

            var result = await controller.FundingStreamInclusionExclusion(vm, action: "");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(RolloverController.EnterRolloverEligibilityDates), redirect.ActionName);

            Assert.True(session.TryGetValue("RolloverSession", out var bytes));
            var json = System.Text.Encoding.UTF8.GetString(bytes);
            var updated = JsonConvert.DeserializeObject<Rollover>(json);

            Assert.NotNull(updated);
            Assert.NotNull(updated.RolloverFundingStream);
            Assert.Single(updated.RolloverFundingStream.SelectedIds);
        }
    }
}
