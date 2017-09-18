using Zenject;

public class GameStateFactory
{

	private DiContainer _container;
	
	public GameStateFactory(DiContainer container)
	{
		_container = container;
	}

	public IGameState Create(string id)
	{
		switch (id)
		{
			case "CoreGame":
				return _container.Instantiate<CoreGameState>();
			default:
				throw new System.NotImplementedException();
		}
	}
}
