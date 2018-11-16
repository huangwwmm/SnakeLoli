using System.Collections;
using UnityEngine.SceneManagement;

public class slSceneFSMState_Lobby : slSceneFSMState
{
	private slUIRoot_Lobby m_LobbyUIRoot;

	public override bool SupportCoroutineChange()
	{
		return true;
	}

	public override IEnumerator Activate_Co()
	{
		if (SceneManager.GetActiveScene().name != slConstants.SCENE_NAME_LOBBY)
		{
			yield return StartCoroutine(LoadScene_Co(slConstants.SCENE_NAME_LOBBY));
		}

		if (m_LobbyUIRoot == null)
		{
			m_LobbyUIRoot = hwmSystem.GetInstance().GetUISystem().InstantiateUIRoot<slUIRoot_Lobby>("Lobby");
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