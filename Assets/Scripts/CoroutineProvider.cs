using UnityEngine;

public class CoroutineProvider : MonoBehaviour 
{
    
	private void OnDestroy()
	{
		Debug.LogWarning("Coroutine provider is destroyed - all coroutines will stop");
		StopAllCoroutines();
	}
}
