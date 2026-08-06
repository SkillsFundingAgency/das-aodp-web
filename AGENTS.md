# Unit Testing Instructions

## Scope

These instructions apply whenever creating, modifying, reviewing, or fixing unit tests in this repository.

Follow the repository's existing structure, naming conventions, coding standards, analyzers, and package versions. Where these instructions conflict with an established project-specific convention, preserve the existing convention unless the task explicitly requires changing it.

## Required Test Stack

- Use **xUnit** as the test framework.
- Structure tests using the **Arrange, Act, Assert (AAA)** pattern.
- Use **Moq** as the mocking framework.
- Prefer **Shouldly** for assertions.
- Do not introduce another testing, mocking, assertion, or test-data generation library unless it is already required by the project.
- Do not introduce AutoFixture unless the repository already mandates its use.

## Test Class Requirements

- Name test classes after the system under test, normally using the format `{TypeUnderTest}Tests`.
- All new test classes must inherit from `UnitTest`.
- Reuse the existing `UnitTest` base class when one is already present in the test project.
- If no `UnitTest` base class exists, create the following class in an appropriate shared location within the test project:

```csharp
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
```

- Do not create duplicate `UnitTest` classes.
- Place the base class in the namespace and folder that best match the existing test-project structure.
- Do not add unrelated shared functionality to `UnitTest` merely for convenience.

## Test Structure

Use explicit AAA sections unless the test is so small that the separation is already unmistakable:

```csharp
[Fact]
public void MethodName_WhenCondition_ExpectedOutcome()
{
    // Arrange

    // Act

    // Assert
}
```

- Each test should verify one behaviour or outcome.
- Keep the Act section focused on a single invocation of the system under test.
- Avoid conditional logic, loops, exception swallowing, or production-like algorithms inside tests.
- A test should fail for one clear reason.
- Prefer readable duplication over abstractions that obscure the scenario.
- Extract test builders, factories, or helper methods only when they materially improve readability and are reused.

## Test Naming

Prefer descriptive names in the following form:

```text
MethodOrMember_WhenScenario_ExpectedOutcome
```

For example:

```csharp
Calculate_WhenEndDateIsBeforeStartDate_ThrowsArgumentException
```

Use the repository's existing naming convention when it is already consistent and differs from this format.

## Facts and Theories

- Use `[Fact]` for a single behaviour or scenario.
- Use `[Theory]` when the same behaviour must be verified against multiple meaningful inputs.
- Prefer `[InlineData]` for simple values.
- Use `MemberData` or `ClassData` only when the data is too complex for `InlineData`.
- Give every theory case a clear purpose; do not add large data matrices that repeat equivalent coverage.
- Do not combine unrelated behaviours into one theory merely to reduce the number of test methods.

## Mocking with Moq

- Prefer Moq mocks over hand-written stubs or fake implementations where a collaborator must be substituted.
- Mock dependencies at the boundary of the system under test, such as repositories, gateways, clients, clocks, queues, and other injected services.
- Do not mock the system under test.
- Do not mock simple data objects, records, value objects, collections, or pure domain entities.
- Do not mock framework internals merely to make a unit test possible.
- Configure only behaviour relevant to the scenario.
- Prefer strongly typed Moq expressions; avoid reflection-based setup.
- Verify interactions only when the interaction itself is part of the required behaviour.
- Do not overuse `VerifyAll()` or `VerifyNoOtherCalls()`. They often couple tests to implementation details and should be used only when all interactions are contractually significant.
- Verify that commands or side effects occur with the correct complete object wherever practical, rather than separately verifying every argument property.
- Capture arguments only when a direct strongly typed verification would be unclear or insufficient.
- Use strict mocks only when strictness improves the test and matches existing project conventions; do not make tests brittle solely for stricter interaction checking.

## Assertions with Shouldly

- Prefer Shouldly assertions for all new tests.
- Do not introduce FluentAssertions or another assertion framework when Shouldly can express the assertion.
- Assert the complete result or object graph wherever possible.
- Prefer a single full-object assertion over a sequence of individual property assertions when the complete expected object can be expressed clearly.
- Use `ShouldBeEquivalentTo` for structural object-graph comparisons when it is supported by the Shouldly version used by the project and its comparison semantics are appropriate for the types involved.
- Use `ShouldBe` when the type implements meaningful value equality, such as records and value objects.
- Construct a complete expected object before the assertion where doing so makes the intended result clearer.
- For collections, assert the complete expected collection where ordering is significant.
- When ordering is not significant, use an appropriate equivalence assertion rather than sorting production results solely for the test.
- Use specific Shouldly assertions such as `ShouldBeNull`, `ShouldNotBeNull`, `ShouldBeEmpty`, `ShouldContain`, and `ShouldThrow` where they make the expected behaviour clearer.
- Assert exception type and meaningful exception details owned by the application. Do not assert framework-generated wording unless that exact wording is part of the application's contract.
- Avoid redundant assertions. For example, do not separately assert non-null when a subsequent assertion already proves it.

Example:

```csharp
var expected = new FundingResult(
    QualificationId: qualificationId,
    IsApproved: true,
    ApprovalEndDate: expectedEndDate);

result.ShouldBe(expected);
```

Or, where structural equivalence is required and supported:

```csharp
result.ShouldBeEquivalentTo(expected);
```

## Coverage Expectations

- Aim for **100% unit-test coverage of application-owned logic where reasonably achievable**.
- Coverage must not be increased by writing tests for .NET or third-party framework behaviour.
- Do not create meaningless tests solely to execute lines.
- Do not weaken production design, expose private members, or add test-only branches to satisfy coverage.
- Cover all meaningful branches, including:
  - successful paths;
  - alternative business paths;
  - validation and guard clauses;
  - null, empty, default, and boundary values where relevant;
  - error and exception paths owned by the application;
  - cancellation behaviour where the application handles cancellation;
  - business-significant dependency interactions;
  - case sensitivity and comparison rules where relevant;
  - date, time, numeric, and collection boundaries where relevant.

When 100% coverage would require testing framework implementation details or behaviour with no application-owned decision, leave that code uncovered and explain the reason in the change summary when appropriate.

## Do Not Test Framework Behaviour

Do not add tests whose primary purpose is to prove that .NET or a third-party framework works as documented.

Examples of tests that should normally not be added include tests proving that:

- automatic properties store and return values;
- constructors assign parameters without any additional application logic;
- records implement compiler-generated equality;
- `List<T>`, LINQ, `DateTime`, `CancellationToken`, or other BCL types behave correctly;
- ASP.NET Core model binding, routing, filters, validation attributes, or dependency injection work as documented;
- Entity Framework Core tracks entities, materialises queries, applies `Include`, or translates LINQ as documented;
- configuration, serialization, logging, or options frameworks perform their standard behaviour;
- Moq, xUnit, or Shouldly behaves correctly.

Test application-owned behaviour built on top of a framework, not the framework itself.

Custom mappings, converters, policies, filters, validators, query composition, middleware behaviour, serialization rules, or dependency-registration logic may be tested when they contain meaningful application-owned decisions. Where correctness depends on a real framework provider or runtime pipeline, use the appropriate integration-test suite instead of mocking framework internals in a unit test.

## Behaviour over Implementation

- Test observable public behaviour rather than private implementation details.
- Do not test private methods directly.
- Do not change private members to `public` or `internal` solely to unit test them.
- Exercise private logic through the public API that owns the behaviour.
- Avoid assertions about incidental call order unless ordering is part of the business contract.
- Avoid tests that break during a safe refactor even though externally observable behaviour is unchanged.

## System Under Test Construction

- Instantiate the system under test explicitly.
- Keep its dependencies visible in the Arrange section or in clearly named test-class fields.
- Prefer fresh mocks and a fresh system-under-test instance per test.
- Do not share mutable state across tests.
- Do not rely on test execution order.
- Keep constructors lightweight and deterministic.
- Follow the existing project convention for common mock fields and system-under-test factories.

## Asynchronous Tests

- Use `async Task`; never use `async void`.
- Await the operation under test.
- Pass the inherited `CancellationToken` when the production API accepts one, unless the scenario specifically requires a different token.
- Use a dedicated cancelled token when testing cancellation.
- Do not use arbitrary delays such as `Task.Delay` to coordinate a unit test.
- Avoid `.Result`, `.Wait()`, and other sync-over-async patterns.

Example:

```csharp
[Fact]
public async Task Handle_WhenRequestIsValid_ReturnsExpectedResult()
{
    // Arrange

    // Act
    var result = await sut.Handle(request, CancellationToken);

    // Assert
    result.ShouldBe(expected);
}
```

## Test Data

- Use explicit test data that communicates why the scenario matters.
- Populate all properties relevant to the behaviour being tested.
- Avoid random values unless randomness is seeded and materially useful.
- Use fixed dates, identifiers, and numeric values so failures are reproducible.
- Prefer builders or factory methods already present in the test project.
- Keep irrelevant values minimal, but do not create invalid object graphs accidentally.
- Use realistic domain values when they improve comprehension.
- Use distinct values for expected and unexpected cases so incorrect property mapping cannot pass unnoticed.

## Time, Randomness, and External Resources

Unit tests must be deterministic and isolated.

- Do not call the real clock when the application provides a clock abstraction.
- Do not access networks, databases, queues, cloud resources, the file system, environment-specific configuration, or other external services.
- Mock the application's abstraction over those resources.
- Do not use the in-memory replacement of a framework as a substitute for testing application logic when a simpler abstraction can be mocked.
- Tests requiring a real database provider, web host, serializer pipeline, or dependency-injection container belong in an integration-test project unless the repository establishes a different convention.

## Guard Clauses and Validation

- Test application-owned validation rules.
- Do not separately test guard-clause library internals.
- For parameter guards, assert the expected exception and parameter name when those are deliberately defined by the application.
- Avoid duplicating equivalent guard tests for compiler-generated or framework-enforced behaviour.

## Collections and Queries

- Verify empty, single-item, and multiple-item scenarios when each can produce different application behaviour.
- Test duplicate handling, ordering, filtering, grouping, and case sensitivity when they are part of the business rule.
- Do not mock `IQueryable`, `DbSet<T>`, or EF Core query providers to imitate SQL translation.
- Test pure query composition against in-memory collections only when the behaviour is provider-independent.
- Use integration tests when the risk concerns EF Core translation or database-provider behaviour.

## Maintaining Existing Tests

When modifying production code:

1. Locate and follow the nearest existing tests for the same feature.
2. Update tests affected by intentional behaviour changes.
3. Add tests for newly introduced branches and regressions.
4. Preserve valid existing coverage.
5. Remove obsolete tests only when their asserted behaviour is intentionally removed.
6. Do not rewrite unrelated tests or change test conventions without a clear need.
7. Run the smallest relevant test set first, then the complete affected test project.
8. Use the repository's existing build, test, and coverage commands.

## Final Validation

Before completing a unit-test change:

- Ensure the test project builds without warnings introduced by the change.
- Run all tests in the affected test project.
- Confirm tests are deterministic and pass when run independently.
- Confirm tests do not depend on execution order or local machine state.
- Review coverage for application-owned code.
- Check that no test was added merely to exercise .NET or third-party framework code.
- Check that new test classes inherit from `UnitTest`.
- Check that assertions use Shouldly where practical.
- Check that mocks use Moq and verify only meaningful interactions.
- Check that object and collection results are asserted as complete graphs where practical.
- Keep production-code changes out of a test-only task unless they are necessary to make application-owned behaviour testable through sound design.