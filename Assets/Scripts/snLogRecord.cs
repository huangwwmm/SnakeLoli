using System;
using System.IO;
using UnityEngine;

public class snLogRecord
{
	private FileStream m_FileStream;
	private StreamWriter m_StreamWriter;

	/// <summary>
	/// avoid multiple registered <see cref="OnLog"/>
	/// </summary>
	private bool m_IsRegisteredOnLogEvent = false;

	private byte m_RecordLogTypeFlags = byte.MaxValue;
	private bool m_RecordStackTrace = true;

	public void Initialize(byte recordLogTypeFlag, bool recordStackTrace)
	{
		m_RecordLogTypeFlags = recordLogTypeFlag;
		m_RecordStackTrace = recordStackTrace;

		string logDirectory;

		string logFile;
#if UNITY_EDITOR || UNITY_STANDALONE
		logDirectory = Application.dataPath + "/../Temp/LogRecord/";
		logFile = DateTime.Now.ToString("yyyy-M-dd--HH-mm-ss") + ".log";
#endif

		string logPath = logDirectory + logFile;
		Debug.Log("LogRecord path: " + logPath);
		try
		{
			if (!Directory.Exists(logDirectory))
			{
				Directory.CreateDirectory(logDirectory);
			}

			m_FileStream = new FileStream(logPath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
			m_StreamWriter = new StreamWriter(m_FileStream);
			m_StreamWriter.AutoFlush = false; // for better performance
			StartReceivedLog();
		}
		catch (Exception e)
		{
			snDebug.Assert(false, "Create LogRecord file failed. Exception:\n" + e.ToString());
		}
	}

	public void Destroy()
	{
		StopReceivedLog();

		FlushStreamWriter();
		m_StreamWriter.Close();
		m_FileStream.Flush();
		m_FileStream.Close();
	}

	/// <summary>
	/// call this function instead of auto flush for better preformance
	/// </summary>
	public void FlushStreamWriter()
	{
		m_StreamWriter.Flush();
	}

	public void StartReceivedLog()
	{
		if (!m_IsRegisteredOnLogEvent)
		{
			Application.logMessageReceived += OnLog;
			m_IsRegisteredOnLogEvent = true;
		}
	}

	public void StopReceivedLog()
	{
		if (m_IsRegisteredOnLogEvent)
		{
			Application.logMessageReceived -= OnLog;
			m_IsRegisteredOnLogEvent = false;
		}
	}

	private void OnLog(string condition, string stackTrace, LogType type)
	{
		if ((m_RecordLogTypeFlags & (1 << (byte)type)) > 0)
		{
			m_StreamWriter.WriteLine(string.Format("{0}|{1}|{2}", DateTime.Now.ToString("G"), type, condition));
			if (m_RecordStackTrace)
			{
				m_StreamWriter.WriteLine(stackTrace);
			}
		}
	}
}