using System.Collections;
using UnityEngine.SceneManagement;

public class slSceneFSMState_Lobby : slSceneFSMState
{
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
	}

	public override IEnumerator Deactivate_Co()
	{
		yield return null;
	}
}