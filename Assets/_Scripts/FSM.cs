
using System;
using System.Collections.Generic;

public class FSM
{
    private State _currentState;
    private Dictionary<State, List<(Func<bool> condition, State target)>> _transitions = new();

    public State CurrentState => _currentState;

    public void SetInitialState(State state)
    {
        _currentState = state;
        _currentState.Enter();
    }

    public void AddTransition(State from, Func<bool> condition, State to)
    {
        if (!_transitions.ContainsKey(from))
            _transitions[from] = new List<(Func<bool>, State)>();

        _transitions[from].Add((condition, to));
    }

    public void UpdateStatesFSM()
    {
        
        if (_transitions.ContainsKey(_currentState))
        {
            foreach (var t in _transitions[_currentState])
            {
                if (t.condition())
                {
                    _currentState.Exit();
                    _currentState = t.target;
                    _currentState.Enter();
                    return; 
                }
            }
        }

        // Acá lo unico que hacemos es que si no cambio de estado que siga ejectuando la actual.
        _currentState.Execute();
    }
}