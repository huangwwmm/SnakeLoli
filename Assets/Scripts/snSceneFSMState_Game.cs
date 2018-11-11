using System.Collections;
using UnityEngine.SceneManagement;

public class snSceneFSMState_Game : snSceneFSMState
{
	public override bool SupportCoroutineChange()
	{
		return true;
	}

	public override IEnumerator Activate_Co()
	{
		if (SceneManager.GetActiveScene().name != snConstants.SCENE_NAME_GAME)
		{
			yield return StartCoroutine(LoadScene_Co(snConstants.SCENE_NAME_GAME));
		}

		snSystem.GetInstance().GetUISystem().InstantiateUIRoot<snGameUIRoot>("Game");
	}

	public override IEnumerator Deactivate_Co()
	{
		snSystem.GetInstance().GetUISystem().DestroyUIRoot("Game");
		yield return null;
	}
}