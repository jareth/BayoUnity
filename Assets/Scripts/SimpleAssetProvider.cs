using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class SimpleAssetProvider : IAssetProvider
{
    private CoroutineProvider _coroutineProvider;

    public SimpleAssetProvider(CoroutineProvider coroutineProvider)
    {
        _coroutineProvider = coroutineProvider;
    }

    public event Action<RootAsset> AssetLoaded = (loadedObject) => { };

    public void LoadAsset(string assetId)
    {
        //CoroutineProvider.Instance.StartCoroutine(LoadAssetAsync(assetId));
        _coroutineProvider.StartCoroutine(LoadAssetAsync(assetId));
    }

    public IEnumerator LoadAssetAsync(string assetId)
    {
        yield return SceneManager.LoadSceneAsync(assetId, LoadSceneMode.Additive);
    }

    public void RegisterAsset(RootAsset rootAsset)
    {
        AssetLoaded(rootAsset);
    }

    public void UnloadAsset(string assetId)
    {
        _coroutineProvider.StartCoroutine(UnloadAssetAsync(assetId));
    }

    public IEnumerator UnloadAssetAsync(string assetId)
    {
        yield return SceneManager.UnloadSceneAsync(assetId);
    }
}