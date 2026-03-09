using UnityEngine;

public enum ELogType
{
    Normal,
    Warning,
    Error
}

/// <summary>
/// MonoBehaviour 기반 싱글톤 베이스 클래스
/// </summary>
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    /// <summary>
    /// 싱글톤 인스턴스 저장 변수
    /// 전역에서 하나의 인스턴스만 유지하기 위해 static 으로 선언
    /// </summary>
    private static T _instance;
    /// <summary>
    /// 멀티스레드 환경에서 동시 접근 방지용 락 객체
    /// lock(_lock) 으로 여러 스레드가 동시에 Instance 에 접근할 때 중복 생성을 방지함
    /// readonly 로 선언하여 락 객체 자체가 변경되지 않도록 보장
    /// </summary>
    protected static readonly object _lock = new object();
    /// <summary>
    /// 애플리케이션 종료 플래그
    /// Unity 는 종료 시 오브젝트 파괴 순서가 불확정적이기에
    /// 종료 중에 Instance 접근 시 새 오브젝트가 생성되는 것을 방지
    /// OnApplicationQuit() 에서 true 설정됨
    /// </summary>
    private static bool _isApplicationQuitting = false;

    [Header("Utility")]
    [SerializeField] protected bool _bDebug = true;           // 로그 출력여부

    /// <summary>
    /// 프로퍼티
    /// 싱글톤 인스턴스 접근자
    /// </summary>
    public static T Instance
    {
        get
        {
            if (true == _isApplicationQuitting)
            {
                return null;
            }

            lock (_lock)
            {
                if (null == _instance)
                {
                    // 순서 상관없이 빠르게 찾기(싱글톤에 적합)
                    _instance = FindAnyObjectByType<T>();

                    if (null == _instance)
                    {
                        // 새 GameObject 생성 및 컴포넌트 추가
                        GameObject singletonObj = new GameObject();
                        _instance = singletonObj.AddComponent<T>();
                        singletonObj.name = $"[Singleton] {typeof(T)}";

                        // 씬 전환시 객체 유지
                        DontDestroyOnLoad(singletonObj);
                    }
                }

                return _instance;
            }
        }
    }

    /// <summary>
    /// 인스턴스 존재 여부 확인 (인스턴스 생성 없이)
    /// </summary>
    public static bool HasInstance => null != _instance;


    #region 유니티 이벤트 함수 + 추상 함수

    /// <summary>
    /// 싱글톤 초기화
    /// </summary>
    protected virtual void Awake()
    {
        if (null == _instance)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
            OnSingletonAwake();
        }
        else if (this != _instance)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 싱글톤 초기화 시 호출되는 가상 메서드
    /// 서브클래스에서 오버라이드하여 초기화 로직 구현 
    /// </summary>
    protected abstract void OnSingletonAwake();

    /// <summary>
    /// 앱 종료 시 자동 호출 
    /// </summary>
    private void OnApplicationQuit()
    {
        _isApplicationQuitting = true;
        OnSingletonApplicationQuit();
    }

    /// <summary>
    /// 서브클래스에서 오버라이드하여 로직 구현
    /// </summary>
    protected abstract void OnSingletonApplicationQuit();

    /// <summary>
    /// 싱글톤 파괴
    /// 오브젝트 파괴 시 자동호출
    /// </summary>
    private void OnDestroy()
    {
        if (this == _instance)
        {
            OnSingletonDestroy();
            _instance = null;
        }
    }

    /// <summary>
    /// 싱글톤 파괴 시 호출되는 가상 메서드
    /// 자식클래스에서 오버라이드하여 정리 로직 구현  
    /// </summary>
    protected abstract void OnSingletonDestroy();

    #endregion


    #region Utility 함수

    // 로그 출력
    protected void Log(string msg, ELogType type = ELogType.Normal)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (false == _bDebug) return;

        if (ELogType.Normal == type)
            Debug.Log(msg);
        else if (ELogType.Warning == type)
            Debug.LogWarning(msg);
        else if (ELogType.Error == type)
            Debug.LogError(msg);
#endif
    }

    #endregion

}

