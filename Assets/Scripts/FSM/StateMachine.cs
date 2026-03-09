using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    private IState _currentState;
    private Dictionary<Type, IState> _states = new Dictionary<Type, IState>();
    public event Action<IState, IState> OnStateChanged; // 상태 전환 이벤트 (oldState, newState)

    public IState CurrentState => _currentState;

    #region 유니티 이벤트 함수

    private void Update()
    {
        // 현재 상태의 Update 호출
        _currentState?.Update();
    }

    #endregion

    #region 외부 호출 함수

    /// <summary>
    /// 상태 등록
    /// </summary>
    public void AddState<T>(T state) where T : IState
    {
        var type = typeof(T);
        if (!_states.ContainsKey(type))
        {
            _states[type] = state;
        }
    }

    /// <summary>
    /// 상태 전환
    /// </summary>
    public void ChangeState<T>() where T : IState
    {
        // 제네릭 타입 T의 실제 Type 정보 획득
        var type = typeof(T);

        // Dictionary에서 해당 타입의 State 검색
        if (!_states.TryGetValue(type, out IState newState))
        {
            Debug.LogError($"[StateMachine] State not found: {type.Name}");
            return;
        }

        // 같은 상태면 무시
        if (_currentState == newState) return;

        var oldState = _currentState;

        // 상태 전환 수행 : Exit → Enter
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();

        // 외부에 상태 변경 알림
        OnStateChanged?.Invoke(oldState, _currentState);

        Debug.Log($"[StateMachine] {oldState?.GetType().Name ?? "None"} → {_currentState.GetType().Name}");
    }

    /// <summary>
    /// 특정 타입 상태 가져오기
    /// </summary>
    public T GetState<T>() where T : IState
    {
        var type = typeof(T);
        if (_states.TryGetValue(type, out IState state))
        {
            return (T)state;
        }
        return default;
    }

    /// <summary>
    /// 현재 상태가 특정 타입인지 확인
    /// </summary>
    public bool IsCurrentState<T>() where T : IState
    {
        return _currentState is T;
    }

    #endregion

}