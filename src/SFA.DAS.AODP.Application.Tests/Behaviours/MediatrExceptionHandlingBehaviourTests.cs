using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Behaviours;
using SFA.DAS.AODP.Application.Exceptions;
using SFA.DAS.AODP.Application.Queries.Review.Rollover;
using SFA.DAS.AODP.Domain.Rollover;
using Shouldly;

namespace SFA.DAS.AODP.Application.UnitTests.Behaviours;

public class MediatrExceptionHandlingBehaviourTests
{
    [Fact]
    public async Task Handle_WhenRequestThrowsException_ShouldWrapExceptionWithRequestName()
    {
        // Arrange
        var request = new GetAwardingOrganisationsForRolloverQueryBuilderQuery(RolloverQueryBuilderRequest.Builder().Build());
        var exception = new InvalidOperationException("API failed");
        var behaviour = new MediatrExceptionHandlingBehaviour<GetAwardingOrganisationsForRolloverQueryBuilderQuery, BaseMediatrResponse<GetAwardingOrganisationsForRolloverQueryBuilderQueryResponse>>();
        RequestHandlerDelegate<BaseMediatrResponse<GetAwardingOrganisationsForRolloverQueryBuilderQueryResponse>> next = _ => Task.FromException<BaseMediatrResponse<GetAwardingOrganisationsForRolloverQueryBuilderQueryResponse>>(exception);

        // Act
        var result = await Should.ThrowAsync<MediatrRequestException>(() => behaviour.Handle(request, next, CancellationToken.None));

        // Assert
        result.RequestName.ShouldBe(nameof(GetAwardingOrganisationsForRolloverQueryBuilderQuery));
        result.InnerException.ShouldBe(exception);
    }
}
