using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class snConstants
{
	public const string SCENE_NAME_LOBBY = "Lobby";
	public const string SCENE_NAME_GAME = "Game";

	public enum CanvasSortOrder : short
	{
		JoystickCursor = 30000
	}

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