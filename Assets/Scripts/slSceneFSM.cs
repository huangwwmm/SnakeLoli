﻿using System.Collections;
using UnityEngine.SceneManagement;

public class slSceneFSM : hwmSceneFSM
{
	public override IEnumerator EnterStartupScene()
	{
		string startupScene = SceneManager.GetActiveScene().name;

		if (startupScene == slConstants.SCENE_NAME_GAME)
		{
			hwmSystem.GetInstance().SetWaitingToPlayLevel(
				hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Level, slConstants.DEFAULT_LEVEL_NAME) as hwmLevel);
		}
		yield return StartCoroutine(ChangeState_Co(FindState(startupScene)));
	}
}