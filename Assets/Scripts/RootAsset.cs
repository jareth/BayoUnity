using UnityEngine;
using Zenject;

public class RootAsset : MonoBehaviour
{
    
    [Inject]
    void Init(IAssetProvider assetProvider)
    {
        assetProvider.RegisterAsset(this);
    }
}