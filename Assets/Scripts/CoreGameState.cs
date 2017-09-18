using System;
using System.Collections;
using System.Collections.Generic;

public class CoreGameState : IGameState
{

	private IAssetProvider _assetProvider;
	private List<string> _assetsToLoad;
	
	public CoreGameState(IAssetProvider assetProvider, List<string> assetsToLoad)
	{
		_assetProvider = assetProvider;
		_assetsToLoad = assetsToLoad;
	}
    
	public IEnumerator Load()
	{
		_assetProvider.AssetLoaded += StoreLoadedAsset;
		foreach (string assetId in _assetsToLoad)
		{
			yield return _assetProvider.LoadAssetAsync(assetId);
		}
	}

	private void StoreLoadedAsset(RootAsset rootAsset)
	{
	}

	public void Enter()
	{
	}

	public void Pause()
	{
		throw new NotImplementedException();
	}

	public void Resume()
	{
		throw new NotImplementedException();
	}

	public void Exit()
	{
	}

	public IEnumerator Unload()
	{
		foreach (string assetId in _assetsToLoad)
		{
			yield return _assetProvider.UnloadAssetAsync(assetId);
		}
	}
}
