using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class snSystem : MonoBehaviour
{
	private static snSystem ms_Instance;

	public snSystemInitializer SystemInitializer;

	private snLogRecord m_LogRecord;
	private snINIParser m_Config;
	private snInput m_Input;
	private snSceneFSM m_SceneFSM;
	private float m_RealtimeSinceStartup;

	public static snSystem GetInstance()
	{
		return ms_Instance;
	}

	public snInput GetInput()
	{
		return m_Input;
	}

	public snLogRecord GetLogRecord()
	{
		return m_LogRecord;
	}

	public float GetRealtimeSinceStartup()
	{
		return m_RealtimeSinceStartup;
	}

	protected void Awake()
	{
		if (ms_Instance == null) // avoid multiple snSystem
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
		if (ms_Instance == this) // avoid multiple snSystem
		{
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
		// initialize log record
		m_LogRecord = new snLogRecord();
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

		// initialize config
		m_Config = new snINIParser();
		m_Config.Initialize(SystemInitializer.Config.text);
		System.Text.StringBuilder configLogString = new System.Text.StringBuilder(512);
		configLogString.AppendLine("Loaded Config");
		foreach (KeyValuePair<string, string> kv in m_Config.GetConfig())
		{
			configLogString.AppendLine(string.Format("{0}={1}", kv.Key, kv.Value));
		}
		Debug.Log(configLogString.ToString());
		yield return null;

		// initialize input
		m_Input = InstantiatePrefabAndSetParentThisTransform<snInput>(SystemInitializer.InputPrefab);
		yield return null;

		// initialize scene fsm
		m_SceneFSM = InstantiatePrefabAndSetParentThisTransform<snSceneFSM>(SystemInitializer.SceneFSMPrefab);
		yield return StartCoroutine(m_SceneFSM.ChangeState_Co(m_SceneFSM.GetLobbyState()));
		yield return null;
	}

	private T InstantiatePrefabAndSetParentThisTransform<T>(GameObject prefab)
	{
		GameObject obj = Instantiate(prefab) as GameObject;
		obj.transform.SetParent(transform, false);
		return obj.GetComponent<T>();
	}
}

[System.Serializable]
public class snSystemInitializer
{
	public TextAsset Config;
	public GameObject InputPrefab;
	public GameObject SceneFSMPrefab;
}