using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class slGameMode_Free : hwmGameMode
{
	private Vector2 m_SnakeSpawnMinPosition;
	private Vector2 m_SnakeSpawnMaxPosition;

	public void DoUpdateRespawnPlayer()
	{
		if (slWorld.GetInstance().GetGameState().GetMatchState() == hwmMatchState.InProgress)
		{
			List<hwmPlayerState> playerStates = slWorld.GetInstance().GetGameState().GetPlayerStates();
			for (int iPlayerState = 0; iPlayerState < playerStates.Count; iPlayerState++)
			{
				slPlayerState_Free iterPlayerState = playerStates[iPlayerState] as slPlayerState_Free;

				if (iterPlayerState.ControllerSnake == null
					&& Time.time - iterPlayerState.LastDeadTime > slWorld.GetInstance().GetLevel().RespawnTime)
				{
					SpawnPlayer(iterPlayerState);
					return;
				}
			}
		}
	}

	protected override IEnumerator HandleStartPlay_Co()
	{
		hwmObserver.OnActorDestroy += HandleActorDestroy;
		yield return StartCoroutine(slWorld.GetInstance().GetFoodSystem().EnterMap_Co());

		m_SnakeSpawnMinPosition = slWorld.GetInstance().GetMap().GetMapBounds().min + new Vector2(slConstants.SNAKE_SPAWN_SAFEAREA_MAP_EDGE, slConstants.SNAKE_SPAWN_SAFEAREA_MAP_EDGE);
		m_SnakeSpawnMaxPosition = slWorld.GetInstance().GetMap().GetMapBounds().max - new Vector2(slConstants.SNAKE_SPAWN_SAFEAREA_MAP_EDGE, slConstants.SNAKE_SPAWN_SAFEAREA_MAP_EDGE);

		int maxBot = slWorld.GetInstance().GetLevel().BotCount;
		for (int iBot = 0; iBot < maxBot; iBot++)
		{
			// TEMP PlayerState
			slPlayerState_Free iterBotState = new slPlayerState_Free();
			iterBotState.IsBot = true;
			iterBotState.PlayerName = "Bot_" + iBot.ToString();
			iterBotState.PlayerID = iBot;
			iterBotState.SnakeName = "10000";
			slWorld.GetInstance().GetGameState().AddPlayerState(iterBotState);
			SpawnPlayer(iterBotState);

			// for aviod snakes together update move
			yield return null;
		}

		slPlayerState_Free playerState = new slPlayerState_Free();
		playerState.IsBot = false;
		playerState.PlayerName = "Player";
		playerState.PlayerID = 1000;
		playerState.SnakeName = "40000";
		slWorld.GetInstance().GetGameState().AddPlayerState(playerState);
		SpawnPlayer(playerState);
	}

	protected override IEnumerator HandleEndPlay_Co()
	{
		yield return null;
		hwmObserver.OnActorDestroy -= HandleActorDestroy;
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
		initializeAdditionalData.NodeCount = 5;
		playerState.ControllerSnake = slWorld.GetInstance().CreateActor("Snake_" + playerState.PlayerID.ToString()
			, playerState.PlayerID
			, "Snake_" + playerState.SnakeName
			, Vector3.zero
			, Quaternion.identity
			, initializeAdditionalData) as slSnake;
	}

	private void CalculateSpawnPoint(slPlayerState_Free playerState, out Vector3 position, out Quaternion rotation)
	{
		float angle = hwmRandom.RandFloat(0, 360);// TODO calculate spawn rotation
		rotation = Quaternion.Euler(0, 0, angle);
		position = hwmRandom.RandVector2(m_SnakeSpawnMinPosition, m_SnakeSpawnMaxPosition);

		// UNDONE check safe
	}

	private void HandleActorDestroy(hwmActor actor)
	{
		if (actor is slSnake)
		{
			slPlayerState_Free playerState = slWorld.GetInstance().GetGameState().FindPlayerStateByPlayerID(actor.GetGuid()) as slPlayerState_Free;
			playerState.LastDeadTime = Time.time;
		}
	}
}