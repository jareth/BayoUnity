using System.Collections;

public interface IGameStateMachine
{
    void RegisterState(string id, IGameState gameState);
    IEnumerator GoToState(string gameStateId); // normally use Enums
}
