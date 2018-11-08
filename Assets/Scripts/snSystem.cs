using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class snSystem : MonoBehaviour
{
	private static snSystem ms_Instance;

	public Object[] SystemPrefabs;

	private snLogRecord m_LogRecord;
	private snINIParser m_Config;
	private float m_RealtimeSinceStartup;

	public static snSystem GetInstance()
	{
		return ms_Instance;
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
		string[] configLines;
#if UNITY_EDITOR
		configLines = System.IO.File.ReadAllLines(Application.dataPath + "/Config/Debug.ini", System.Text.Encoding.UTF8);
#endif
		m_Config = new snINIParser();
		m_Config.Initialize(configLines);
		System.Text.StringBuilder configLogString = new System.Text.StringBuilder(512);
		configLogString.AppendLine("Loaded Config");
		foreach (KeyValuePair<string, string> kv in m_Config.GetConfig())
		{
			configLogString.AppendLine(string.Format("{0}={1}", kv.Key, kv.Value));
		}
		Debug.Log(configLogString.ToString());

		// initialize system prefab
		for (int iPrefab = 0; iPrefab < SystemPrefabs.Length; iPrefab++)
		{
			Object iterPrefab = SystemPrefabs[iPrefab];
			GameObject obj = Instantiate(iterPrefab) as GameObject;
			obj.transform.SetParent(transform, false);
			yield return null;
		}
	}
}