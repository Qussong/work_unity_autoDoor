using UnityEngine;

public class BaseState<TState, TView> : IState where TView : BaseView
{
    protected TView _view;

    public BaseState(TView view)
    {
        _view = view;
    }

    public virtual void Enter()
    {
        Debug.Log($"[{typeof(TState).Name}] Enter");
        _view.Show();
    }

    public virtual void Exit()
    {
        Debug.Log($"[{typeof(TState).Name}] Exit");
        _view.Hide();
    }

    public virtual void Update()
    {
        //
    }
}
