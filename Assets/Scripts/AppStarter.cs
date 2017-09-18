using Zenject;

public class AppStarter : IInitializable
{
    private CoroutineProvider _coroutineProvider;
    private IGameStateMachine _gameStateMachine;
    private GameStateFactory _gameStateFactory;

    public AppStarter(CoroutineProvider coroutineProvider, IGameStateMachine gameStateMachine, GameStateFactory gameStateFactory)
    {
        _coroutineProvider = coroutineProvider;
        _gameStateMachine = gameStateMachine;
        _gameStateFactory = gameStateFactory;
    }
    
    public void Initialize()
    {
        SetupGame();
        StartGame();
    }

    public void SetupGame()
    {
        _gameStateMachine.RegisterState("CoreGame", _gameStateFactory.Create("CoreGame"));
    }

    public void StartGame()
    {
        _coroutineProvider.StartCoroutine(_gameStateMachine.GoToState("CoreGame"));
    }
}
