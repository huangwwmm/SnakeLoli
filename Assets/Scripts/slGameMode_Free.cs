using System.Collections;
using UnityEngine;

public class slGameMode_Free : hwmGameMode
{
	private Vector2 m_SnakeSpawnMinPosition;
	private Vector2 m_SnakeSpawnMaxPosition;

	protected override IEnumerator HandleStartPlay_Co()
	{
		yield return StartCoroutine(slWorld.GetInstance().GetFoodSystem().EnterMap_Co());

		m_SnakeSpawnMinPosition = slWorld.GetInstance().GetMap().GetMapBounds().min + slConstants.SNAKE_SPAWN_SAFEAREA_MAP_EDGE;
		m_SnakeSpawnMaxPosition = slWorld.GetInstance().GetMap().GetMapBounds().max - slConstants.SNAKE_SPAWN_SAFEAREA_MAP_EDGE;

		int maxBot = (int)(slWorld.GetInstance().GetLevel().MapSize.x * slWorld.GetInstance().GetLevel().MapSize.x / slWorld.GetInstance().GetLevel().BotDensity);
		for (int iBot = 0; iBot < maxBot; iBot++)
		{
			// TEMP PlayerState
			slPlayerState_Free iterBotState = new slPlayerState_Free();
			iterBotState.IsBot = true;
			iterBotState.PlayerName = "Bot_" + iBot.ToString();
			iterBotState.PlayerID = iBot;
			iterBotState.SnakeName = "Snake_10000";
			slWorld.GetInstance().GetGameState().AddPlayerState(iterBotState);
			SpawnPlayer(iterBotState);

			// for aviod snakes together update move
			yield return null;
			yield return null;
			yield return null;
		}

		slPlayerState_Free playerState = new slPlayerState_Free();
		playerState.IsBot = false;
		playerState.PlayerName = "Player";
		playerState.PlayerID = 1000;
		playerState.SnakeName = "Snake_10000";
		slWorld.GetInstance().GetGameState().AddPlayerState(playerState);
		SpawnPlayer(playerState);
	}

	private void SpawnPlayer(slPlayerState_Free playerState)
	{
		if (playerState.ControllerSnake != null)
		{
			throw new System.Exception();
		}

		Vector3 spawnPosition;
		Quaternion spawnRotation;
		CalculateSpawnPoint(playerState, out spawnPosition, out spawnRotation);
		slSnake.InitializeAdditionalData initializeAdditionalData = new slSnake.InitializeAdditionalData();
		initializeAdditionalData.HeadPosition = spawnPosition;
		initializeAdditionalData.HeadRotation = spawnRotation;
		initializeAdditionalData.IsBot = playerState.IsBot;
		initializeAdditionalData.TweakableProperties = slConstants.DEFAULT_SNAKE_TWEAKABLE_PROPERTIES;
		initializeAdditionalData.NodeCount = 50;
		playerState.ControllerSnake = slWorld.GetInstance().CreateActor("Snake_" + playerState.PlayerID.ToString()
			, playerState.SnakeName
			, Vector3.zero
			, Quaternion.identity
			, initializeAdditionalData) as slSnake;
	}

	private void CalculateSpawnPoint(slPlayerState_Free playerState, out Vector3 position, out Quaternion rotation)
	{
		float angle = hwmRandom.RandFloat(0, 360);// TODO calculate spawn rotation
		rotation = Quaternion.Euler(0, 0, angle);
		position = hwmRandom.NextVector2(m_SnakeSpawnMinPosition, m_SnakeSpawnMaxPosition);

		// UNDONE check safe
	}
}