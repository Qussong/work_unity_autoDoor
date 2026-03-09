using UnityEngine;

public class NavigationManager : MonoSingleton<NavigationManager>
{
    /// <summary>
    /// 필드
    /// </summary>
    [Header("Views")]
    [SerializeField] private StartView _startView;
    //[SerializeField] private ContentView _contentView;
    //[SerializeField] private ResultView _resultView;

    /// <summary>
    /// 프로퍼티
    /// </summary>
    public StateMachine StateMachine { get; private set; }

    #region 유니티 이벤트 함수

    protected override void OnSingletonAwake()
    {
        // StateMachine 컴포넌트 추가
        StateMachine = gameObject.AddComponent<StateMachine>();

        // 상태 등록
        RegisterState();

        // 상태 변경 이벤트 구독
        StateMachine.OnStateChanged += HandleStateChanged;
    }

    private void Start()
    {
        // 초기 상태
        GoTo<StartState>();
    }

    protected override void OnSingletonApplicationQuit()
    {
    }

    protected override void OnSingletonDestroy()
    {
        if (StateMachine != null)
        {
            StateMachine.OnStateChanged -= HandleStateChanged;
        }
    }

    #endregion

    #region 내부 호출 함수

    private void RegisterState()
    {
        StateMachine.AddState(new StartState(_startView));
        //StateMachine.AddState(new ContentState(_contentView));
        //StateMachine.AddState(new ResultState(_resultView));
    }

    private void HandleStateChanged(IState oldState, IState newState)
    {
        // IdleManager 타이머 리셋
        IdleManager.Instance?.ResetTimer();

        // 상태 변경 시 추가 처리

    }

    #endregion

    #region 외부 호출 함수

    public void GoTo<T>() where T : IState
    {
        StateMachine.ChangeState<T>();
    }

    #endregion

}