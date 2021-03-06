﻿using System.Collections;
using UnityEngine.SceneManagement;

public class slSceneFSMState_Game : slSceneFSMState
{
	public override bool SupportCoroutineChange()
	{
		return true;
	}

	public override IEnumerator Activate_Co()
	{
		if (SceneManager.GetActiveScene().name != slConstants.SCENE_NAME_GAME)
		{
			yield return StartCoroutine(LoadScene_Co(slConstants.SCENE_NAME_GAME));
		}

		yield return StartCoroutine(hwmWorld.GetInstance().BeginPlay_Co());
		// UNDONE move to player.cs
		//hwmSystem.GetInstance().GetUISystem().InstantiateUIRoot<hwmGameUIRoot>("Game");
	}

	public override IEnumerator Deactivate_Co()
	{
		//hwmSystem.GetInstance().GetUISystem().DestroyUIRoot("Game");
		yield return StartCoroutine(hwmWorld.GetInstance().EndPlay_Co());
	}
}