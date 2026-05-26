namespace SFA.DAS.AODP.Web.UnitTests.Testing;

/// <summary>
/// Base class for all tests in this project to provide common functionality and context for the tests.
/// </summary>
public class UnitTest
{
    /// <summary>
    /// Provides a context object for the currently executing test, allowing access to test-specific information such as test name, test properties, and more.
    /// </summary>
#pragma warning disable CA1822
    protected ITestContext CurrentContext => TestContext.Current;
#pragma warning restore CA1822

    /// <summary>
    /// Provides access to a <see cref="CancellationToken"/> from the <see cref="TestContext"/>
    /// </summary>
    protected CancellationToken CancellationToken => CurrentContext.CancellationToken;
}