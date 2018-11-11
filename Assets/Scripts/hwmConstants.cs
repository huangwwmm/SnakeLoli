using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hwmConstants
{
	public const int CANVAS_SORTORDER_JOYSTICKCURSOR = 30000;

	/// <summary>
	/// The order of the enum value will affect the UI display
	/// </summary>
	public enum SupportLanguage : byte
	{
		English,
		Chinese,
	}

	public const SupportLanguage DEFAULT_LANGUAGE = SupportLanguage.English;

	public const string PREFSKEY_LANGUAGE = "Language";
}