using System.Collections.Generic;
using UnityEngine;

public class hwmINIParser
{
	private Dictionary<string, string> m_Config;

	public void Initialize(string configText, char commentSymbol = ';')
	{
		m_Config = new Dictionary<string, string>();

		string[] lines = configText.Split('\n');
		for (int iLine = 0; iLine < lines.Length; iLine++)
		{
			string iterLine = lines[iLine].Trim();
			// ignore empty line
			if (string.IsNullOrEmpty(iterLine))
			{
				continue;
			}

			// 注释
			if (iterLine[0] == commentSymbol)
			{
				continue;
			}
			else
			{
				int splitIndex = iterLine.IndexOf('=');
				if (splitIndex > 0)
				{
					string key = iterLine.Substring(0, splitIndex);
					string value = iterLine.Substring(splitIndex + 1);
					if (m_Config.ContainsKey(key))
					{
						Debug.LogWarning(string.Format("key({0}) already exists in settingini", key));
						m_Config[key] = value;
					}
					else
					{
						m_Config.Add(key, value);
					}
				}
				else
				{
					Debug.LogWarning(string.Format("({0}) not a valid ini line", iterLine));
				}
			}
		}
	}

	public void Dispose()
	{
		m_Config.Clear();
		m_Config = null;
	}

	public Dictionary<string, string> GetConfig()
	{
		return m_Config;
	}

	public bool TryGetValue(string key, out string value, bool ignoreLog = false)
	{
		bool sucess = m_Config.TryGetValue(key, out value);
		if (!sucess)
		{
			if (!ignoreLog)
			{
				Debug.LogWarning(string.Format("key({0}) not exists in settingini", key));
			}
		}
		return sucess;
	}

	public bool TryGetIntValue(string key, out int value, bool ignoreLog = false)
	{
		string sValue;
		if (TryGetValue(key, out sValue))
		{
			if (int.TryParse(sValue, out value))
			{
				return true;
			}
			else
			{
				if (!ignoreLog)
				{
					Debug.LogWarning(string.Format("key({0})->value({1}) cant convert to int", key, sValue));
				}
				return false;
			}
		}
		else
		{
			value = 0;
			return false;
		}
	}

	public bool TryGetBoolValue(string key, out bool value, bool ignoreLog = false)
	{
		string sValue;
		if (TryGetValue(key, out sValue))
		{
			if (bool.TryParse(sValue, out value))
			{
				return true;
			}
			else
			{
				if (!ignoreLog)
				{
					Debug.LogWarning(string.Format("key({0})->value({1}) cant convert to bool", key, sValue));
				}
				return false;
			}
		}
		else
		{
			value = false;
			return false;
		}
	}
}