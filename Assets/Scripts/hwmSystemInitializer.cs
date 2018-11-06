using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hwmSystemInitializer : MonoBehaviour
{
	private static bool ms_AlreadyInitialized = false;

	public Object[] SystemPrefabs;

	private bool m_Initialized = false;

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
		for (int iPrefab = 0; iPrefab < SystemPrefabs.Length; iPrefab++)
		{
			Object iterPrefab = SystemPrefabs[iPrefab];
			GameObject obj = Instantiate(iterPrefab) as GameObject;
			obj.transform.parent = transform;
			yield return null;
		}

		m_Initialized = true;
		ms_AlreadyInitialized = true;
	}

	protected void OnDestroy()
	{
		if (m_Initialized)
		{
		}
		else
		{
			// dont need do anything
		}
	}
}