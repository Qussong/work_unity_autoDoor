using UnityEngine;

public class ResultState : BaseState<ResultState, ResultView>
{
    public ResultState(ResultView view) : base(view)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            NavigationManager.Instance.GoTo<StartState>();
        }
    }
}
