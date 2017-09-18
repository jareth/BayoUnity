public class GameFlowDirector 
{
    private readonly CoroutineProvider _coroutineProvider;
    private readonly IGameStateMachine _gameStateMachine;

    public GameFlowDirector(CoroutineProvider coroutineProvider, IGameStateMachine gameStateMachine)
    {
        _coroutineProvider = coroutineProvider;
        _gameStateMachine = gameStateMachine;
    }
}
