using System;
using System.Collections.Generic;
using UnityEngine;

public class hwmLocalization
{
	/// <summary>
	/// {0} => Localization key
	/// </summary>
	private const string LOCALIZATION_PLACEHOLDER = "${0}$";

	private Dictionary<string, string> m_Localization;
	private TextAsset m_CSVAsset;
	private hwmConstants.SupportLanguage m_Language = hwmConstants.SupportLanguage.Chinese;

	public void Initialize(TextAsset csvAsset)
	{
		m_CSVAsset = csvAsset;
		m_Localization = new Dictionary<string, string>();

		ParseLocalization();
	}

	public void Dispose()
	{
		m_Localization.Clear();
		m_Localization = null;

		m_CSVAsset = null;
	}

	public void ReParseLocalizationIfLanguageChanged()
	{
		m_Localization.Clear();
		ParseLocalization();
	}

	public string GetLocalizationText(string key)
	{
		string text;
		if (!TryGetText(key, out text))
		{
			text = string.Format(LOCALIZATION_PLACEHOLDER, key);
		}
		return text;
	}

	public bool TryGetText(string key, out string text)
	{
		return m_Localization.TryGetValue(key, out text);
	}

	private void ParseLocalization()
	{
		string[][] csvTable = CSVParser.Parse(m_CSVAsset.text);
		hwmDebug.Assert(csvTable.Length > 0, "csvTable Length == 0");

		int targetColumn = -1;
		// 第0行为表头
		string[] csvHeader = csvTable[0];
		string language = m_Language.ToString();
		for (int iColumn = 0; iColumn < csvHeader.Length; iColumn++)
		{
			if (csvHeader[iColumn] == language)
			{
				targetColumn = iColumn;
				break;
			}
		}
		hwmDebug.Assert(targetColumn >= 0, string.Format("not found language({0}) in localization.csv", language));

		// zero row is header
		for (int iRow = 1; iRow < csvTable.Length; iRow++)
		{
			string[] iterRow = csvTable[iRow];
			if (iterRow.Length < targetColumn)
			{
				Debug.LogError(string.Format("The {0} row of the localization.csv has fewer than {1} columns", iRow + 1, targetColumn + 1));
			}

			string iterKey = iterRow[0];
			string iterText = iterRow[targetColumn];

			if (string.IsNullOrEmpty(iterKey))
			{
				continue;
			}
			else if (string.IsNullOrEmpty(iterText))
			{
				Debug.LogError(string.Format("Row 5 Column 6 of the localization.csv is empty", iRow + 1, targetColumn + 1));
			}

			if (m_Localization.ContainsKey(iterKey))
			{
				Debug.LogWarning(string.Format("key({0}) already exists in localization", iterKey));
			}
			else
			{
				m_Localization.Add(iterKey, iterText);
			}
		}

		Debug.Log(string.Format("localization csv parse finish. language: {0} count: {1}", language, m_Localization.Count));
	}

	/// <summary>
	/// Priority:
	///		Cache in PlayerPrebs
	///		System Language
	/// </summary>
	private hwmConstants.SupportLanguage GetLanguage()
	{

		if (PlayerPrefs.HasKey(hwmConstants.PREFSKEY_LANGUAGE))
		{
			string languageInPrefs = PlayerPrefs.GetString(hwmConstants.PREFSKEY_LANGUAGE);
			hwmConstants.SupportLanguage language;
			if (IsSupportLanguage(languageInPrefs, out language))
			{
				Debug.Log(string.Format("language cache ({0}) detected", languageInPrefs));
				return language;
			}
		}
		if (Application.platform == RuntimePlatform.Android
			|| Application.platform == RuntimePlatform.IPhonePlayer
			|| Application.platform == RuntimePlatform.tvOS
			|| Application.platform == RuntimePlatform.WindowsEditor)
		{
			string sysLanguage = Application.systemLanguage.ToString();
			hwmConstants.SupportLanguage language;
			if (IsSupportLanguage(sysLanguage, out language))
			{
				Debug.Log(string.Format("system language ({0}) detected", sysLanguage));
				return language;
			}
		}

		if (Application.systemLanguage == SystemLanguage.Chinese
			|| Application.systemLanguage == SystemLanguage.ChineseSimplified
			|| Application.systemLanguage == SystemLanguage.ChineseTraditional)
		{
			return hwmConstants.SupportLanguage.Chinese;
		}

		return hwmConstants.DEFAULT_LANGUAGE;
	}

	private bool IsSupportLanguage(string sLanguage, out hwmConstants.SupportLanguage language)
	{
		try
		{
			language = (hwmConstants.SupportLanguage)Enum.ToObject(typeof(hwmConstants.SupportLanguage), sLanguage);
			return true;
		}
		catch (Exception)
		{
			language = 0;
			return false;
		}
	}
}