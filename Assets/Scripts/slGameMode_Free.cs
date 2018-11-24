using System.Collections;
using UnityEngine;

public class slGameMode_Free : hwmGameMode
{
	protected override IEnumerator HandleStartPlay_Co()
	{
		yield return StartCoroutine(slWorld.GetInstance().GetFoodSystem().EnterMap_Co());

		int maxBot = (int)(slWorld.GetInstance().GetLevel().MapSize.x * slWorld.GetInstance().GetLevel().MapSize.x / slWorld.GetInstance().GetLevel().BotDensity);
		for (int iBot = 0; iBot < maxBot; iBot++)
		{
			slPlayerState_Free playerState = new slPlayerState_Free();
			playerState.IsBot = true;
			playerState.PlayerName = "TEST"; // TEMP Name
			playerState.PlayerID = iBot; // TEMP ID

			slSnake.InitializeAdditionalData initializeAdditionalData = new slSnake.InitializeAdditionalData();
			initializeAdditionalData.HeadPosition = new Vector3(3, 5, 0);
			initializeAdditionalData.HeadRotation = Quaternion.Euler(0, 0, 60);
			playerState.ControllerSnake = slWorld.GetInstance().CreateActor("Snake_" + playerState.PlayerID.ToString()
				, "Snake_10000" // TEMP Snake Prefab
				, Vector3.zero
				, Quaternion.identity
				, initializeAdditionalData) as slSnake;

			slWorld.GetInstance().GetGameState().AddPlayerState(playerState);
		}
	}
}