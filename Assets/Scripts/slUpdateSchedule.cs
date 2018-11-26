using System;
using System.Collections.Generic;
using UnityEngine;

public class slUpdateSchedule : MonoBehaviour
{
	private List<SnakeUpdteScheduleParmeter> m_Snakes;
	private float m_UpdateSnakeMovementTime = 0;
	private int m_UpdateRespawnPlayerFrame = 0;
	private int m_LastUpdateAISnakeIndex = -1;

	protected void Awake()
	{
		m_Snakes = new List<SnakeUpdteScheduleParmeter>();

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
				SnakeUpdteScheduleParmeter snake = m_Snakes[iSnake];
				snake.Owner.DoUpdateMovement(1, slConstants.UPDATE_SNAKE_MOVEMENT_TIEM_INTERVAL);
				snake.NeedUpdateAI = true;
			}
		}

		if (!currentUpdateSnakeMovement)
		{
			// update ai
			if (m_Snakes.Count > 0)
			{
				int needUpdateAICount = Mathf.CeilToInt(m_Snakes.Count / slConstants.UPDATE_ALL_AI_FRAME);
				int currentAIIndex = m_LastUpdateAISnakeIndex;
				int whileTime = 0;
				while (whileTime++ < m_Snakes.Count
					|| needUpdateAICount == 0)
				{
					currentAIIndex = GetNextSnakeIndex(currentAIIndex);

					SnakeUpdteScheduleParmeter snake = m_Snakes[currentAIIndex];
					if (snake.NeedUpdateAI
						&& snake.Owner.GetController() != null
						&& snake.Owner.GetController().IsAI())
					{
						snake.NeedUpdateAI = false;
						(snake.Owner.GetController() as slAIController).DoAIUpdate();
						m_LastUpdateAISnakeIndex = currentAIIndex;
						needUpdateAICount--;
					}
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

	private void OnActorCreate(hwmActor actor)
	{
		if (actor is slSnake)
		{
			SnakeUpdteScheduleParmeter snakeParmeter = new SnakeUpdteScheduleParmeter();
			snakeParmeter.Owner = actor as slSnake;
			snakeParmeter.NeedUpdateAI = true;
			m_Snakes.Add(snakeParmeter);
		}
	}

	private void OnActorDestroy(hwmActor actor)
	{
		if (actor is slSnake)
		{
			slSnake snake = actor as slSnake;
			for (int iSnake = 0; iSnake < m_Snakes.Count; iSnake++)
			{
				if (m_Snakes[iSnake].Owner == snake)
				{
					m_Snakes.RemoveAt(iSnake);
					if (m_LastUpdateAISnakeIndex == iSnake)
					{
						m_LastUpdateAISnakeIndex--;
					}
					break;
				}
			}
		}
	}

	private int GetNextSnakeIndex(int index)
	{
		return index >= m_Snakes.Count - 1
			? 0
			: index + 1;
	}

	private struct SnakeUpdteScheduleParmeter
	{
		public slSnake Owner;
		public bool NeedUpdateAI;
	}
}