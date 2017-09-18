using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MainInstaller : MonoInstaller
{

	[SerializeField] private CoroutineProvider CoroutineProviderInstance;

	public override void InstallBindings()
	{
		Container.Bind<CoroutineProvider>().FromInstance(CoroutineProviderInstance);
		Container.Bind<IAssetProvider>().To<SimpleAssetProvider>().AsSingle();

		Container.Bind<IGameStateMachine>().To<SimlpeGameStateMachine>().AsSingle();
		Container.Bind<GameStateFactory>().AsTransient();
		Container.Bind<GameFlowDirector>().AsSingle().NonLazy();

		Container.Bind<IInitializable>().To<AppStarter>().AsSingle();

		Container.Bind<List<string>>()
			.FromInstance(new List<string> { "CubeScene" })
			.WhenInjectedInto<CoreGameState>();
	}
}