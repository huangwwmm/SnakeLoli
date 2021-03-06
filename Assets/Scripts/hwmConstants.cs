﻿public class hwmConstants
{
	public const float KINDA_SMALL_NUMBER = 0.0001f;

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

	/// <summary>
	/// first enum value must equal 0 and auto increment for preformance
	/// </summary>
	public enum ButtonIndex
	{
		Notset = -1,
		Skill1,
		Skill2,
		Skill3,
		Skill4,
		Skill5,
		Skill6,
		Menu,
	}

	/// <summary>
	/// first enum value must equal 0 and auto increment for preformance
	/// </summary>
	public enum AxisIndex
	{
		Notset = -1,
		MoveX,
		MoveY,
	}

	public enum NetRole
	{
		/// <summary>
		/// No role at all.
		/// </summary>
		None,
		/// <summary>
		/// Locally simulated proxy of this actor.
		/// </summary>
		SimulatedProxy,
		/// <summary>
		/// Locally autonomous proxy of this actor.
		/// </summary>
		AutonomousProxy,
		/// <summary>
		/// Authoritative control over the actor.
		/// </summary>
		Authority,
	}
}