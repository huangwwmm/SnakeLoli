using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class snSceneFSMState_Lobby : snSceneFSMState
{
	public GameObject LobbyUIPrefab;

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
	}

	public override IEnumerator Deactivate_Co()
	{
		yield return null;
	}
}