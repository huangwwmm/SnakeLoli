using System.Collections;
using UnityEngine.SceneManagement;

public class slSceneFSM : hwmSceneFSM
{
	public override IEnumerator EnterStartupScene()
	{
		string startupScene;
#if UNITY_EDITOR
		startupScene = SceneManager.GetActiveScene().name;
#else
		startupScene = slConstants.SCENE_NAME_LOBBY;
#endif
		yield return StartCoroutine(ChangeState_Co(FindState(startupScene)));
	}
}