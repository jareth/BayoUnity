using System.Collections;
using System.Collections.Generic;

public class SimlpeGameStateMachine : IGameStateMachine
{
    
    private Dictionary<string, IGameState> _gameStates;
    private IGameState _activeState;

    public SimlpeGameStateMachine()
    {
        _gameStates = new Dictionary<string, IGameState>();
    }
    
    public void RegisterState(string id, IGameState gameState)
    {
        _gameStates.Add(id, gameState);
    }

    public IEnumerator GoToState(string gameStateId)
    {
        if (_activeState != null)
        {
            _activeState.Exit();
            yield return _activeState.Unload();
        }
        
        var nextState = _gameStates[gameStateId];
        yield return nextState.Load();
        
        nextState.Enter();
        _activeState = nextState;
    }
}
