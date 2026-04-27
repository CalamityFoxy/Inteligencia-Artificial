
using System;
using System.Collections.Generic;
public enum EnemyStateType
{
    Idle,
    Chase,
    Attack,
    Patroll,
    ReturnToBase,
    Flee,
    Heal
}

public class FSM
{

    private State _currentState;
    private Dictionary<EnemyStateType, State> _states = new();

    public State CurrentState => _currentState;

    public void RegisterState(EnemyStateType type, State state)
    {
        _states[type] = state;
    }

    public void SetInitialState(EnemyStateType type)
    {
        _currentState = _states[type];
        _currentState.Enter();
    }

    public void SetState(EnemyStateType type)
    {
        var newState = _states[type];

        // El guard que mencionamos: si ya estoy ahí, no hago nada
        if (_currentState == newState) return;

        _currentState.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public void Execute()
    {
        _currentState?.Execute();
    }
}