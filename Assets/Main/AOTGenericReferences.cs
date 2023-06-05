using UnityEngine;

public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ constraint implement type
	// }} 

	// {{ AOT generic type
	// }}

	public void RefMethods()
	{
		// System.Object UnityEngine.AssetBundle::LoadAsset<System.Object>(System.String)
		
		var go = new UnityEngine.GameObject().AddComponent<AudioSource>();
	}
}