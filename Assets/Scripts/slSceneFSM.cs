public class slSceneFSM : hwmFSM
{
	private slSceneFSMState_Lobby m_LobbyState;
	private slSceneFSMState_Game m_GameState;

	public slSceneFSMState_Lobby GetLobbyState()
	{
		return m_LobbyState;
	}
	public slSceneFSMState_Game GetGameState()
	{
		return m_GameState;
	}

	protected override void Awake()
	{
		base.Awake();

		m_LobbyState = FindState("Lobby") as slSceneFSMState_Lobby;
		m_GameState = FindState("Game") as slSceneFSMState_Game;
	}

	protected override void OnDestroy()
	{
		m_GameState = null;
		m_LobbyState = null;

		base.OnDestroy();
	}
}