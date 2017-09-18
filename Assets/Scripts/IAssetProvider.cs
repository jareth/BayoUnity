using System;
using System.Collections;

public interface IAssetProvider
{
	event Action<RootAsset> AssetLoaded;
	void LoadAsset(string assetId);
	IEnumerator LoadAssetAsync(string assetId);
	void RegisterAsset(RootAsset rootAsset);
	void UnloadAsset(string assetId);
	IEnumerator UnloadAssetAsync(string assetId);
}
