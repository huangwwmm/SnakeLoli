using System.Collections;
using UnityEngine.SceneManagement;

public class snSceneFSMState_Lobby : snSceneFSMState
{
	private snLobbyUIRoot m_LobbyUIRoot;

	public override bool SupportCoroutineChange()
	{
		return true;
	}

	public override IEnumerator Activate_Co()
	{
		if (SceneManager.GetActiveScene().name != snConstants.SCENE_NAME_LOBBY)
		{
			yield return StartCoroutine(LoadScene_Co(snConstants.SCENE_NAME_LOBBY));
		}

		if (m_LobbyUIRoot == null)
		{
			m_LobbyUIRoot = snSystem.GetInstance().GetUISystem().InstantiateUIRoot<snLobbyUIRoot>("Lobby");
		}
		else
		{
			m_LobbyUIRoot.OnUIRootDisplay();
		}
	}

	public override IEnumerator Deactivate_Co()
	{
		m_LobbyUIRoot.OnUIRootHide();
		yield return null;
	}
}