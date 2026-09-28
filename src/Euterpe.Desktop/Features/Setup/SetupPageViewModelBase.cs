namespace Euterpe.Features.Setup;

public abstract class SetupPageViewModelBase : ViewModelBase
{
    public abstract LocalizedString Title { get; }

    public virtual bool CanGoNext => true;

    public virtual bool CanGoBack => true;

    public virtual LocalizedString NextButtonText => Button_Next;

    public required SetupState State { get; init; }

    public virtual Task OnEnterAsync() => Task.CompletedTask;
}
