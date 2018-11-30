using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class slUpdateSchedule : MonoBehaviour
{
	private List<slSnake> m_Snakes;
	private float m_UpdateSnakeMovementTime = 0;
	private int m_UpdateRespawnPlayerFrame = 0;
	private float m_LastUpdateFoodsTime = 0;

	private IEnumerator m_DoUpdateEnumerator;
	private float m_Time;
	private float m_DeltaTime;

	private bool m_EnableUpdateStatistics;
	private UpdateStatistics[] m_UpdateStatisticss;
	private System.Diagnostics.Stopwatch m_UpdateStatisticsStopwatch;

	public void LogStatistics()
	{
		if (m_EnableUpdateStatistics)
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			stringBuilder.AppendLine("Update Statistics:");
			for (int iUpdate = 0; iUpdate < m_UpdateStatisticss.Length; iUpdate++)
			{
				UpdateStatistics iterUpdate = m_UpdateStatisticss[iUpdate];
				stringBuilder.AppendLine(string.Format("{0}: Count:{1} AvgTicks:{2:f2}"
					, (UpdateType)iUpdate
					, iterUpdate.UpdateCount
					, iterUpdate.UpdateTicks / (double)iterUpdate.UpdateCount));
			}
			Debug.Log(stringBuilder.ToString());
		}
	}

	protected void Awake()
	{
		m_Snakes = new List<slSnake>();

		hwmObserver.OnActorCreate += OnActorCreate;
		hwmObserver.OnActorDestroy += OnActorDestroy;

		m_Time = 0;
		m_DoUpdateEnumerator = DoUpdate();

		hwmSystem.GetInstance().GetConfig().TryGetBoolValue("Enable_UpdateStatistics", out m_EnableUpdateStatistics);
		if (m_EnableUpdateStatistics)
		{
			m_UpdateStatisticss = new UpdateStatistics[(int)UpdateType.Count];
			m_UpdateStatisticsStopwatch = new System.Diagnostics.Stopwatch();
		}
	}

	protected void OnDestroy()
	{
		if (m_EnableUpdateStatistics)
		{
			m_UpdateStatisticss = null;
			m_UpdateStatisticsStopwatch.Stop();
			m_UpdateStatisticsStopwatch = null;
		}

		m_DoUpdateEnumerator = null;

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

		m_UpdateSnakeMovementTime += m_DeltaTime;

		if (m_EnableUpdateStatistics)
		{
			m_UpdateStatisticsStopwatch.Reset();
			m_UpdateStatisticsStopwatch.Start();
			m_DoUpdateEnumerator.MoveNext();
			m_UpdateStatisticsStopwatch.Stop();
			int updateType = (int)m_DoUpdateEnumerator.Current;
			m_UpdateStatisticss[updateType].UpdateCount++;
			m_UpdateStatisticss[updateType].UpdateTicks += m_UpdateStatisticsStopwatch.ElapsedTicks;
		}
		else
		{
			m_DoUpdateEnumerator.MoveNext();
		}
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
		while (true)
		{
			if (m_UpdateSnakeMovementTime >= slConstants.UPDATE_SNAKE_MOVEMENT_TIEM_INTERVAL)
			{
				// update snake movement
				m_UpdateSnakeMovementTime -= slConstants.UPDATE_SNAKE_MOVEMENT_TIEM_INTERVAL;
				hwmDebug.Assert(m_UpdateSnakeMovementTime < slConstants.UPDATE_SNAKE_MOVEMENT_TIEM_INTERVAL
					, "m_UpdateSnakeMovementTime < slConstants.UPDATE_SNAKE_MOVEMENT_TIEM_INTERVAL");

				for (int iSnake = 0; iSnake < m_Snakes.Count; iSnake++)
				{
					slSnake snake = m_Snakes[iSnake];
					snake.DoUpdateMovement(slConstants.UPDATE_SNAKE_MOVEMENT_TIEM_INTERVAL);
				}
				slWorld.GetInstance().GetSnakeSystem().GetQuadtree().MergeAndSplitAllNode();
				yield return UpdateType.SnakeMovement;

				// update snake eat food
				for (int iSnake = 0; iSnake < m_Snakes.Count; iSnake++)
				{
					slSnake iterSnake = m_Snakes[iSnake];
					iterSnake.DoUpdateEatFood();
				}
				yield return UpdateType.SnakeEatFood;

				// update foods
				slWorld.GetInstance().GetFoodSystem().DoUpdateFoods(m_Time - m_LastUpdateFoodsTime);
				m_LastUpdateFoodsTime = m_Time;
				yield return UpdateType.Foods;

				// update food system			
				slWorld.GetInstance().GetFoodSystem().DoUpdateFoodSystem();
				yield return UpdateType.FoodSystem;

				// update snake ai
				for (int iSnake = 0; iSnake < m_Snakes.Count; iSnake++)
				{
					slSnake iterSnake = m_Snakes[iSnake];
					if (iterSnake.GetController() != null
						&& iterSnake.GetController().IsAI())
					{
						(iterSnake.GetController() as slAIController).DoAIUpdate();
					}
				}
				yield return UpdateType.SnakeAI;

				// update respawn player

				if (++m_UpdateRespawnPlayerFrame >= slConstants.UPDATE_RESPAWN_FRAME_INTERVAL)
				{
					m_UpdateRespawnPlayerFrame -= slConstants.UPDATE_RESPAWN_FRAME_INTERVAL;
					slWorld.GetInstance().GetGameMode().DoUpdateRespawnPlayer();
				}
				yield return UpdateType.RespawnPlayer;
			}
			else
			{
				yield return UpdateType.Empty;
			}
		}
	}

	private enum UpdateType
	{
		Empty = 0,
		SnakeMovement,
		SnakeEatFood,
		Foods,
		FoodSystem,
		SnakeAI,
		RespawnPlayer,
		/// <summary>
		/// must end
		/// </summary>
		Count,
	}

	private struct UpdateStatistics
	{
		public int UpdateCount;
		public long UpdateTicks;
	}
}