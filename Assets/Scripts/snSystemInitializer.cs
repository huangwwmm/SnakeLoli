using System.Collections;
using UnityEngine;

/// <summary>
/// add <see cref="snSystem"/> and system required prefab
/// </summary>
public class snSystemInitializer : MonoBehaviour
{
	private static bool ms_AlreadyInitialized = false;

	public Object[] SystemPrefabs;

	protected void Awake()
	{
		if (!ms_AlreadyInitialized)
		{
			DontDestroyOnLoad(this);
			StartCoroutine(Initialize_Co());
		}
		else
		{
			DestroyImmediate(this);
		}
	}

	private IEnumerator Initialize_Co()
	{
		gameObject.AddComponent<snSystem>();
		yield return null;

		for (int iPrefab = 0; iPrefab < SystemPrefabs.Length; iPrefab++)
		{
			Object iterPrefab = SystemPrefabs[iPrefab];
			GameObject obj = Instantiate(iterPrefab) as GameObject;
			obj.transform.SetParent(transform, false);
			yield return null;
		}

		ms_AlreadyInitialized = true;
		DestroyImmediate(this);
	}

	protected void OnDestroy()
	{
	}
}