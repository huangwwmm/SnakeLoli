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