using System;
using System.Collections.Generic;
using UnityEngine;

public class slUpdateSchedule : MonoBehaviour
{
	private List<slSnake> m_Snakes;
	private float m_UpdateSnakeMovementTime = 0;
	private int m_UpdateRespawnPlayerFrame = 0;
	private int m_UpdateAIFrame = 0;
	private int m_LastUpdateAISnakeIndex = -1;

	protected void Awake()
	{
		m_Snakes = new List<slSnake>();

		hwmObserver.OnActorCreate += OnActorCreate;
		hwmObserver.OnActorDestroy += OnActorDestroy;
	}

	protected void OnDestroy()
	{
		hwmObserver.OnActorCreate -= OnActorCreate;
		hwmObserver.OnActorDestroy -= OnActorDestroy;

		m_Snakes.Clear();
		m_Snakes = null;
	}

	protected void FixedUpdate()
	{
		bool currentUpdateSnakeMovement = false;

		// update snake movement
		m_UpdateSnakeMovementTime += Time.deltaTime;
		if (m_UpdateSnakeMovementTime >= slConstants.UPDATE_SNAKE_MOVEMENT_TIEM_INTERVAL)
		{
			currentUpdateSnakeMovement = true;
			m_UpdateSnakeMovementTime -= slConstants.UPDATE_SNAKE_MOVEMENT_TIEM_INTERVAL;
			for (int iSnake = 0; iSnake < m_Snakes.Count; iSnake++)
			{
				m_Snakes[iSnake].DoUpdateMovement(slConstants.UPDATE_SNAKE_MOVEMENT_TIEM_INTERVAL);
			}
		}


		if (!currentUpdateSnakeMovement)
		{
			// update ai
			if (m_Snakes.Count > 0
				&& ++m_UpdateAIFrame >= slConstants.UPDATE_AI_FRAME_INTERVAL)
			{
				m_UpdateAIFrame -= slConstants.UPDATE_AI_FRAME_INTERVAL;
				int currentAIIndex = m_LastUpdateAISnakeIndex;
				int whileTime = 0;
				while (true)
				{
					currentAIIndex = GetNextSnakeIndex(currentAIIndex);
					if (currentAIIndex == m_LastUpdateAISnakeIndex
						&& whileTime != 0)
					{
						break;
					}

					slSnake snake = m_Snakes[currentAIIndex];
					if (snake.GetController() != null
						&& snake.GetController().IsAI())
					{
						(snake.GetController() as slAIController).DoAIUpdate();

						m_LastUpdateAISnakeIndex = currentAIIndex;
						break;
					}

					whileTime++;
				}
			}


			// update respawn player
			if (++m_UpdateRespawnPlayerFrame >= slConstants.UPDATE_RESPAWN_FRAME_INTERVAL)
			{
				m_UpdateRespawnPlayerFrame -= slConstants.UPDATE_RESPAWN_FRAME_INTERVAL;
				slWorld.GetInstance().GetGameMode().DoUpdateRespawnPlayer();
			}
		}
	}

	private void OnActorDestroy(hwmActor actor)
	{
		if (actor is slSnake)
		{
			int snakeIndexOf = m_Snakes.IndexOf(actor as slSnake);
			m_Snakes.RemoveAt(snakeIndexOf);
			if (m_LastUpdateAISnakeIndex == snakeIndexOf)
			{
				m_LastUpdateAISnakeIndex--;
			}
		}
	}

	private void OnActorCreate(hwmActor actor)
	{
		if (actor is slSnake)
		{
			m_Snakes.Add(actor as slSnake);
		}
	}

	private int GetNextSnakeIndex(int index)
	{
		return index >= m_Snakes.Count - 1
			? 0
			: index + 1;
	}
}