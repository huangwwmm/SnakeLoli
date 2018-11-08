using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class snSceneFSMState : snFSMState 
{
	protected IEnumerator LoadScene_Co(string sceneName)
	{
		AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
		while (!async.isDone)
		{
			yield return null;
		}
		Debug.Log(string.Format("Load scene ({0}) complete.", sceneName));
	}
}