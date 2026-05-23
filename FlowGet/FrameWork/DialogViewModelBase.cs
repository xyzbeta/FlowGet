using FlowGet.ViewModels;
using System;


namespace FlowGet.FrameWork;

public abstract partial class DialogViewModelBase<T> : ViewModelBase
{
    public T? DialogResult { get; private set; }

    public event EventHandler? Closed;

    public void Close(T? dialogResult = default)
    {
        DialogResult = dialogResult;
        Closed?.Invoke(this, EventArgs.Empty);
    }

}

public abstract class DialogViewModelBase : DialogViewModelBase<bool?>
{

}
