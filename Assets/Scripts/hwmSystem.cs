using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hwmSystem : MonoBehaviour
{
	private static hwmSystem ms_Instance;

	public hwmSystemInitializer SystemInitializer;

	private hwmLogRecord m_LogRecord;
	private hwmINIParser m_Config;
	private hwmInput m_Input;
	private hwmSceneFSM m_SceneFSM;
	private hwmLocalization m_Localization;
	private hwmUISystem m_UISystem;
	private hwmAssetLoader m_AssetLoader;
	private float m_RealtimeSinceStartup;

	#region Get function
	public static hwmSystem GetInstance()
	{
		return ms_Instance;
	}

	public hwmInput GetInput()
	{
		return m_Input;
	}

	public hwmLogRecord GetLogRecord()
	{
		return m_LogRecord;
	}

	public float GetRealtimeSinceStartup()
	{
		return m_RealtimeSinceStartup;
	}

	public hwmUISystem GetUISystem()
	{
		return m_UISystem;
	}

	public hwmAssetLoader GetAssetLoader()
	{
		return m_AssetLoader;
	}

	public hwmLocalization GetLocalization()
	{
		return m_Localization;
	}
	#endregion // Get function

	protected void Awake()
	{
		if (ms_Instance == null) // avoid multiple System
		{
			ms_Instance = this;
			DontDestroyOnLoad(this);

			StartCoroutine(Initialize_Co());
		}
		else
		{
			DestroyImmediate(this);
		}
	}

	protected void OnDestroy()
	{
		if (ms_Instance == this) // avoid multiple System
		{
			m_SceneFSM = null;

			m_UISystem.Destroy();
			m_UISystem = null;

			m_Localization.Destroy();
			m_Localization = null;

			m_AssetLoader = null;
			
			m_Input = null;

			m_Config.Destroy();
			m_Config = null;

			m_LogRecord.Destroy();
			m_LogRecord = null;

			ms_Instance = null;
		}
	}

	protected void Update()
	{
		m_RealtimeSinceStartup = Time.realtimeSinceStartup;

		m_LogRecord.FlushStreamWriter();
	}

	private IEnumerator Initialize_Co()
	{
		m_LogRecord = new hwmLogRecord();
#if UNITY_EDITOR
		m_LogRecord.Initialize(byte.MaxValue, true);
#else
		m_LogRecord.Initialize((1 << (byte)LogType.Assert)
				| (1 << (byte)LogType.Assert)
				| (1 << (byte)LogType.Exception)
				| (1 << (byte)LogType.Error)
				| (1 << (byte)LogType.Warning)
				| (1 << (byte)LogType.Log)
			, false);
#endif
		yield return null;

		m_Config = new hwmINIParser();
		m_Config.Initialize(SystemInitializer.Config.text);
		System.Text.StringBuilder configLogString = new System.Text.StringBuilder(512);
		configLogString.AppendLine("Loaded Config");
		foreach (KeyValuePair<string, string> kv in m_Config.GetConfig())
		{
			configLogString.AppendLine(string.Format("{0}={1}", kv.Key, kv.Value));
		}
		Debug.Log(configLogString.ToString());
		yield return null;

		m_Input = InstantiatePrefabAndSetParentThisTransform<hwmInput>(SystemInitializer.InputPrefab);
		yield return null;

		m_AssetLoader = new hwmAssetLoader();
		yield return null;

		m_Localization = new hwmLocalization();
		m_Localization.Initialize(SystemInitializer.Localization);
		yield return null;

		m_UISystem = new hwmUISystem();
		m_UISystem.Initialize();
		yield return null;

		m_SceneFSM = InstantiatePrefabAndSetParentThisTransform<hwmSceneFSM>(SystemInitializer.SceneFSMPrefab);
		yield return StartCoroutine(m_SceneFSM.EnterStartupScene());
	}

	private T InstantiatePrefabAndSetParentThisTransform<T>(GameObject prefab)
	{
		GameObject obj = Instantiate(prefab) as GameObject;
		obj.transform.SetParent(transform, false);
		return obj.GetComponent<T>();
	}
}

[System.Serializable]
public class hwmSystemInitializer
{
	public TextAsset Config;
	public GameObject InputPrefab;
	public GameObject SceneFSMPrefab;
	public TextAsset Localization;
}