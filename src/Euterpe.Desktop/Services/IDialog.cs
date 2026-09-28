namespace Euterpe.Services;

public interface IDialog<TResult>
{
    event EventHandler<TResult>? RequestClose;
    void Close(TResult result);
}
