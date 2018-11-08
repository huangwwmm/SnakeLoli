public class snSceneFSM : snFSM
{
	private snSceneFSMState_Lobby m_LobbyState;
	private snSceneFSMState_Game m_GameState;

	public snSceneFSMState_Lobby GetLobbyState()
	{
		return m_LobbyState;
	}
	public snSceneFSMState_Game GetGameState()
	{
		return m_GameState;
	}

	protected override void Awake()
	{
		base.Awake();

		m_LobbyState = FindState("Lobby") as snSceneFSMState_Lobby;
		m_GameState = FindState("Game") as snSceneFSMState_Game;
	}
}