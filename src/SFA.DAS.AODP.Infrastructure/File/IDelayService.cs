namespace SFA.DAS.AODP.Infrastructure.File
{
    public interface IDelayService
    {
        Task DelayAsync(TimeSpan duration, CancellationToken cancellationToken = default);
    }

    public class DelayService : IDelayService
    {
        public Task DelayAsync(TimeSpan duration, CancellationToken cancellationToken = default) =>
            Task.Delay(duration, cancellationToken);
    }
}
