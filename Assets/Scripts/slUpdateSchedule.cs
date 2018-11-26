using System;
using UnityEngine;

public class slUpdateSchedule : MonoBehaviour
{
	private hwmFreeList<slSnake> m_Snakes;
	private float m_UpdateSnakeMovementTime = 0;
	private int m_UpdateRespawnPlayerFrame = 0;

	protected void Awake()
	{
		m_Snakes = new hwmFreeList<slSnake>();

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

		if (!currentUpdateSnakeMovement
			 && ++m_UpdateRespawnPlayerFrame >= slConstants.UPDATE_RESPAWN_FRAME_INTERVAL)
		{
			m_UpdateRespawnPlayerFrame -= slConstants.UPDATE_RESPAWN_FRAME_INTERVAL;
			slWorld.GetInstance().GetGameMode().DoUpdateRespawnPlayer();
		}
	}

	private void OnActorDestroy(hwmActor actor)
	{
		if (actor is slSnake)
		{
			m_Snakes.Remove(actor as slSnake);
		}
	}

	private void OnActorCreate(hwmActor actor)
	{
		if (actor is slSnake)
		{
			m_Snakes.Add(actor as slSnake);
		}
	}
}