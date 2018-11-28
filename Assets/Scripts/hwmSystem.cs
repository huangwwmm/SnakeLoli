using System;
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
	private hwmAssetLoader m_AssetLoader;
	private hwmLevel m_WaitingToPlayLevel;
	private float m_RealtimeSinceStartup;
	private hwmIPerformanceStatistics m_PerformanceStatistics;

	#region get/set function
	public static hwmSystem GetInstance()
	{
		return ms_Instance;
	}

	public hwmIPerformanceStatistics GetPerformanceStatistics()
	{
		return m_PerformanceStatistics;
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

	public hwmAssetLoader GetAssetLoader()
	{
		return m_AssetLoader;
	}

	public hwmLocalization GetLocalization()
	{
		return m_Localization;
	}

	public hwmLevel GetWaitingToPlayLevel()
	{
		return m_WaitingToPlayLevel;
	}

	public void SetWaitingToPlayLevel(hwmLevel level)
	{
		m_WaitingToPlayLevel = level;
	}
	#endregion // get/set function

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
			DestroyImmediate(m_SceneFSM);
			m_SceneFSM = null;

			m_Localization.Dispose();
			m_Localization = null;

			DestroyImmediate(m_Input);
			m_Input = null;

			m_Config.Dispose();
			m_Config = null;

			m_LogRecord.Dispose();
			m_LogRecord = null;

			m_AssetLoader = null;

			m_PerformanceStatistics.Dispose();
			m_PerformanceStatistics = null;

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
		if (SystemInitializer.EnablePerformanceStatistics)
		{
			m_PerformanceStatistics = new hwmPerformanceStatistics();
		}
		else
		{
			m_PerformanceStatistics = new hwmEmptyPerformanceStatistics();
		}
		m_PerformanceStatistics.Initialize();

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

		m_AssetLoader = new hwmAssetLoader();
		yield return null;

		m_Config = new hwmINIParser();
		TextAsset configTextAsset = m_AssetLoader.LoadAsset(hwmAssetLoader.AssetType.System, SystemInitializer.ConfigAssetName) as TextAsset;
		m_Config.Initialize(configTextAsset.text);
		System.Text.StringBuilder configLogString = new System.Text.StringBuilder(512);
		configLogString.AppendLine("Loaded Config");
		foreach (KeyValuePair<string, string> kv in m_Config.GetConfig())
		{
			configLogString.AppendLine(string.Format("{0}={1}", kv.Key, kv.Value));
		}
		Debug.Log(configLogString.ToString());
		yield return null;

		m_Input = InstantiatePrefabAndSetParentThisTransform<hwmInput>(m_AssetLoader.LoadAsset(hwmAssetLoader.AssetType.System, SystemInitializer.InputAssetName));
		yield return null;

		m_Localization = new hwmLocalization();
		m_Localization.Initialize(m_AssetLoader.LoadAsset(hwmAssetLoader.AssetType.System, SystemInitializer.LocalizationAssetName) as TextAsset);
		yield return null;

		Instantiate(m_AssetLoader.LoadAsset(hwmAssetLoader.AssetType.Game, "World"));

		m_SceneFSM = InstantiatePrefabAndSetParentThisTransform<hwmSceneFSM>(m_AssetLoader.LoadAsset(hwmAssetLoader.AssetType.System, SystemInitializer.SceneFSMAssetName));
		yield return StartCoroutine(m_SceneFSM.EnterStartupScene());
	}

	private T InstantiatePrefabAndSetParentThisTransform<T>(UnityEngine.Object prefab)
	{
		GameObject obj = Instantiate(prefab) as GameObject;
		obj.transform.SetParent(transform, false);
		return obj.GetComponent<T>();
	}
}

[System.Serializable]
public class hwmSystemInitializer
{
	public string ConfigAssetName;
	public string InputAssetName;
	public string SceneFSMAssetName;
	public string LocalizationAssetName;
	public bool EnablePerformanceStatistics;
}