using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class slUpdateSchedule : MonoBehaviour
{
	private List<slSnake> m_Snakes;
	private float m_UpdateSnakeMovementTime = 0;
	private int m_UpdateRespawnPlayerFrame = 0;
	private int m_FrameCountSinceSnakeMovement = 0;
	private float m_UpdateFoodSystemTime = 0;
	private float m_LastUpdateFoodsTime = 0;

	private hwmPerformanceStatisticsItem m_PerformanceSnakeMovementItem;
	private hwmPerformanceStatisticsItem m_PerformanceFoodSystemItem;
	private hwmPerformanceStatisticsItem m_PerformanceFoodsItem;
	private hwmPerformanceStatisticsItem m_PerformanceSnakeAIItem;
	private hwmPerformanceStatisticsItem m_PerformanceSnakeEatFoodItem;

	private IEnumerator m_DoUpdateEnumerator;
	private float m_Time;
	private float m_DeltaTime;

	protected void Awake()
	{
		m_Snakes = new List<slSnake>();

		hwmObserver.OnActorCreate += OnActorCreate;
		hwmObserver.OnActorDestroy += OnActorDestroy;

		m_PerformanceSnakeAIItem = hwmSystem.GetInstance().GetPerformanceStatistics().LoadOrCreateItem("Update_SnakeAI", true);
		m_PerformanceFoodSystemItem = hwmSystem.GetInstance().GetPerformanceStatistics().LoadOrCreateItem("Update_FoodSystem", true);
		m_PerformanceFoodsItem = hwmSystem.GetInstance().GetPerformanceStatistics().LoadOrCreateItem("Update_Foods", true);
		m_PerformanceSnakeMovementItem = hwmSystem.GetInstance().GetPerformanceStatistics().LoadOrCreateItem("Update_SnakeMovement", true);
		m_PerformanceSnakeEatFoodItem = hwmSystem.GetInstance().GetPerformanceStatistics().LoadOrCreateItem("Update_SnakeEatFood", true);

		m_Time = 0;
		m_DoUpdateEnumerator = DoUpdate();
	}

	protected void OnDestroy()
	{
		m_DoUpdateEnumerator = null;

		m_PerformanceSnakeMovementItem = null;
		m_PerformanceSnakeAIItem = null;
		m_PerformanceFoodSystemItem = null;
		m_PerformanceFoodsItem = null;
		m_PerformanceSnakeEatFoodItem = null;

		hwmObserver.OnActorCreate -= OnActorCreate;
		hwmObserver.OnActorDestroy -= OnActorDestroy;

		m_Snakes.Clear();
		m_Snakes = null;
	}

	protected void FixedUpdate()
	{
		if (hwmWorld.GetInstance().GetGameState().GetMatchState() != hwmMatchState.InProgress)
		{
			return;
		}

		m_DeltaTime = Time.deltaTime;
		m_Time += m_DeltaTime;

		// update snake movement
		m_UpdateSnakeMovementTime += m_DeltaTime;
		if (m_UpdateSnakeMovementTime >= slConstants.UPDATE_SNAKE_MOVEMENT_TIEM_INTERVAL)
		{
			m_UpdateSnakeMovementTime -= slConstants.UPDATE_SNAKE_MOVEMENT_TIEM_INTERVAL;

			hwmSystem.GetInstance().GetPerformanceStatistics().Start(m_PerformanceSnakeMovementItem);
			for (int iSnake = 0; iSnake < m_Snakes.Count; iSnake++)
			{
				slSnake snake = m_Snakes[iSnake];
				snake.DoUpdateMovement(slConstants.UPDATE_SNAKE_MOVEMENT_TIEM_INTERVAL);
			}
			hwmSystem.GetInstance().GetPerformanceStatistics().Finish(m_PerformanceSnakeMovementItem);
		}

		m_DoUpdateEnumerator.MoveNext();
	}

	private void OnActorCreate(hwmActor actor)
	{
		if (actor is slSnake)
		{
			slSnake snake = actor as slSnake;
			m_Snakes.Add(snake);
		}
	}

	private void OnActorDestroy(hwmActor actor)
	{
		if (actor is slSnake)
		{
			slSnake snake = actor as slSnake;
			m_Snakes.Remove(snake);
		}
	}

	private IEnumerator DoUpdate()
	{
		yield return null;

		while (true)
		{
			// update foods
			hwmSystem.GetInstance().GetPerformanceStatistics().Start(m_PerformanceFoodsItem);
			slWorld.GetInstance().GetFoodSystem().DoUpdateFoods(m_Time - m_LastUpdateFoodsTime);
			m_LastUpdateFoodsTime = m_Time;
			hwmSystem.GetInstance().GetPerformanceStatistics().Finish(m_PerformanceFoodsItem);
			yield return null;

			// update food system			
			hwmSystem.GetInstance().GetPerformanceStatistics().Start(m_PerformanceFoodSystemItem);
			slWorld.GetInstance().GetFoodSystem().DoUpdateFoodSystem();
			hwmSystem.GetInstance().GetPerformanceStatistics().Finish(m_PerformanceFoodSystemItem);
			yield return null;

			// update snake ai
			hwmSystem.GetInstance().GetPerformanceStatistics().Start(m_PerformanceSnakeAIItem);
			for (int iSnake = 0; iSnake < m_Snakes.Count; iSnake++)
			{ 
				slSnake iterSnake = m_Snakes[iSnake];
				// update ai
				if (iterSnake.GetController() != null
					&& iterSnake.GetController().IsAI())
				{
					(iterSnake.GetController() as slAIController).DoAIUpdate();
				}
			}
			hwmSystem.GetInstance().GetPerformanceStatistics().Finish(m_PerformanceSnakeAIItem);
			yield return null;

			// update eat food
			hwmSystem.GetInstance().GetPerformanceStatistics().Start(m_PerformanceSnakeEatFoodItem);
			for (int iSnake = 0; iSnake < m_Snakes.Count; iSnake++)
			{
				slSnake iterSnake = m_Snakes[iSnake];
				iterSnake.DoUpdateEatFood();
			}
			hwmSystem.GetInstance().GetPerformanceStatistics().Finish(m_PerformanceSnakeEatFoodItem);
			yield return null;

			// update respawn player
			if (++m_UpdateRespawnPlayerFrame >= slConstants.UPDATE_RESPAWN_FRAME_INTERVAL)
			{
				m_UpdateRespawnPlayerFrame -= slConstants.UPDATE_RESPAWN_FRAME_INTERVAL;
				slWorld.GetInstance().GetGameMode().DoUpdateRespawnPlayer();
			}
			yield return null;
		}
	}
}