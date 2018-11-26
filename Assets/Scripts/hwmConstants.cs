public class hwmConstants
{
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
		Skill1 = 0,
	}

	/// <summary>
	/// first enum value must equal 0 and auto increment for preformance
	/// </summary>
	public enum AxisIndex
	{
		MoveX = 0,
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