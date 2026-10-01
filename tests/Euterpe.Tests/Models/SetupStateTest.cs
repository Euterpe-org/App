namespace Euterpe.Tests.Models;

[Category("SetupStateTests")]
[TestSubject(typeof(SetupState))]
public sealed class SetupStateTest
{
    private static SetupStepState NewStep(SetupStepStatus status = SetupStepStatus.Pending) =>
        new()
        {
            Kinds = SetupOptionKinds.MelonLoader,
            DisplayName = "step",
            Status = status
        };

    [Test]
    public async Task IsRunning_RunningStage_ReturnsTrue()
    {
        var state = new SetupState { Stage = SetupExecutionStage.Running };
        await Assert.That(state.IsRunning).IsTrue();
    }

    [Test]
    public async Task AllSucceeded_FinishedAndAllStepsSucceeded_ReturnsTrue()
    {
        var state = new SetupState();
        state.Steps.Add(NewStep(SetupStepStatus.Succeeded));
        state.Steps.Add(NewStep(SetupStepStatus.Succeeded));
        state.Stage = SetupExecutionStage.Finished;

        await Assert.That(state.AllSucceeded).IsTrue();
    }

    [Test]
    public async Task AllSucceeded_FailedStep_ReturnsFalse()
    {
        var state = new SetupState();
        state.Steps.Add(NewStep(SetupStepStatus.Succeeded));
        state.Steps.Add(NewStep(SetupStepStatus.Failed));
        state.Stage = SetupExecutionStage.Finished;

        await Assert.That(state.AllSucceeded).IsFalse();
    }

    [Test]
    public async Task AllSucceeded_StageNotFinished_ReturnsFalse()
    {
        var state = new SetupState();
        state.Steps.Add(NewStep(SetupStepStatus.Succeeded));
        state.Stage = SetupExecutionStage.Running;

        await Assert.That(state.AllSucceeded).IsFalse();
    }

    [Test]
    public async Task HasFailedSteps_FinishedWithFailedStep_ReturnsTrue()
    {
        var state = new SetupState();
        state.Steps.Add(NewStep(SetupStepStatus.Succeeded));
        state.Steps.Add(NewStep(SetupStepStatus.Failed));
        state.Stage = SetupExecutionStage.Finished;

        await Assert.That(state.HasFailedSteps).IsTrue();
    }

    [Test]
    public async Task HasFailedSteps_StageNotFinished_ReturnsFalse()
    {
        var state = new SetupState();
        state.Steps.Add(NewStep(SetupStepStatus.Failed));
        state.Stage = SetupExecutionStage.Running;

        await Assert.That(state.HasFailedSteps).IsFalse();
    }

    [Test]
    public async Task Reset_FinishedSetupWithSteps_ClearsStepsAndResetsStage()
    {
        var state = new SetupState();
        state.Steps.Add(NewStep());
        state.Stage = SetupExecutionStage.Finished;

        state.Reset();

        using var _ = Assert.Multiple();
        await Assert.That(state.Steps).IsEmpty();
        await Assert.That(state.Stage).IsEqualTo(SetupExecutionStage.NotStarted);
    }
}
