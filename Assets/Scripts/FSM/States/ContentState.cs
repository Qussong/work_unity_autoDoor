using UnityEngine;

public class ContentState : BaseState<ContentState, ContentView>
{
    public ContentState(ContentView view) : base(view)
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
            NavigationManager.Instance.GoTo<ResultState>();
        }
    }
}
