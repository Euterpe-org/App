namespace Euterpe.Tests.Models;

[Category("SetupStepStateTests")]
[TestSubject(typeof(SetupStepState))]
public sealed class SetupStepStateTest
{
    private static SetupStepState NewStep(SetupStepStatus status = SetupStepStatus.Pending) =>
        new()
        {
            Kinds = SetupOptionKinds.MelonLoader,
            DisplayName = "step",
            Status = status
        };

    [Test]
    [Arguments(SetupStepStatus.Pending, false)]
    [Arguments(SetupStepStatus.Running, false)]
    [Arguments(SetupStepStatus.Succeeded, false)]
    [Arguments(SetupStepStatus.Failed, true)]
    public async Task CanRetry_StatusVariations_ReturnsTrueOnlyWhenFailed(SetupStepStatus status, bool expected) =>
        await Assert.That(NewStep(status).CanRetry).IsEqualTo(expected);

    [Test]
    public async Task StatusDisplay_AllStatuses_ReturnsDistinctText()
    {
        var pending = NewStep().StatusDisplay.ToString();
        var running = NewStep(SetupStepStatus.Running).StatusDisplay.ToString();
        var succeeded = NewStep(SetupStepStatus.Succeeded).StatusDisplay.ToString();
        var failed = NewStep(SetupStepStatus.Failed).StatusDisplay.ToString();

        var distinct = new[] { pending, running, succeeded, failed }.Distinct().Count();
        await Assert.That(distinct).IsEqualTo(4);
    }
}
