using System.Collections.Generic;
using UnityEngine;

public class slAIController : slBaseController
{
	private Vector2 m_SafeAreaMinPosition;
	private Vector2 m_SafeAreaMaxPosition;
	private int m_NotChangeDirectionTimes = 0;
	private Quaternion m_ClockwiseDetectAngle;
	private Quaternion m_CounterclockwiseDetectAngle;
#if UNITY_EDITOR
	private List<AIDetectGizmos> m_AIDetectGizmos = new List<AIDetectGizmos>();
#endif

	public override bool IsAI()
	{
		return true;
	}

	public void DoAIUpdate()
	{
		Vector2 targetDirection = m_Snake.TargetMoveDirection;

		if (m_NotChangeDirectionTimes >= slConstants.SNAKE_RANDMOVEMOENT_WHENNOTCHANGED_TIMES)
		{
			if (hwmRandom.RandFloat() < slConstants.SNAKE_RANDMOVEMOENT_WHENNOTCHANGED_PROBABILITY)
			{
				targetDirection = new Vector2(hwmRandom.RandFloat(-1, 1), hwmRandom.RandFloat(-1, 1)).normalized;
			}
			m_NotChangeDirectionTimes = -1;
		}

		CalculateSafeArea(ref targetDirection.x, m_Snake.GetHeadPosition().x, m_SafeAreaMinPosition.x, m_SafeAreaMaxPosition.x);
		CalculateSafeArea(ref targetDirection.y, m_Snake.GetHeadPosition().y, m_SafeAreaMinPosition.y, m_SafeAreaMaxPosition.y);

#if UNITY_EDITOR
		m_AIDetectGizmos.Clear();
#endif
		bool ignorePredict = IsHitPredict();
		int dangerDirection = 0;
		if ((!IsSafe(m_ClockwiseDetectAngle * targetDirection, slConstants.SNAKE_DETECT_DISTANCE, ignorePredict) && dangerDirection++ > -1)
			|| (!IsSafe(m_CounterclockwiseDetectAngle * targetDirection, slConstants.SNAKE_DETECT_DISTANCE, ignorePredict)) && dangerDirection++ > -1
			|| !(IsSafe(targetDirection, slConstants.SNAKE_DETECT_DISTANCE, ignorePredict) && dangerDirection++ > -1))
		{
			Quaternion angle = dangerDirection == 3
				? m_ClockwiseDetectAngle
				: dangerDirection == 1
					? m_CounterclockwiseDetectAngle
					: m_ClockwiseDetectAngle;
			Vector2 currentCalculateDirection = targetDirection;
			for (int iCalculate = 0; iCalculate < slConstants.SNAKE_AI_CALCULATE_TIMES; iCalculate++)
			{
				currentCalculateDirection = angle * currentCalculateDirection;
				bool isSafe = IsSafe(currentCalculateDirection, slConstants.SNAKE_DETECT_DISTANCE, ignorePredict);
#if UNITY_EDITOR
				AIDetectGizmos aIDetectGizmos = new AIDetectGizmos();
				aIDetectGizmos.StartPosition = m_Snake.GetHeadPosition();
				aIDetectGizmos.EndPosition = (Vector2)m_Snake.GetHeadPosition() + currentCalculateDirection * slConstants.SNAKE_DETECT_DISTANCE;
				aIDetectGizmos.IsSafe = isSafe;
				m_AIDetectGizmos.Add(aIDetectGizmos);
#endif
				if (isSafe)
				{
					targetDirection = currentCalculateDirection;
					break;
				}
			}
		}

		m_NotChangeDirectionTimes = m_Snake.TargetMoveDirection == targetDirection.normalized
			? m_NotChangeDirectionTimes + 1 : 0;
		m_Snake.TargetMoveDirection = targetDirection.normalized;
	}

#if UNITY_EDITOR
	protected void OnDrawGizmos()
	{
		for (int iAIDetect = 0; iAIDetect < m_AIDetectGizmos.Count; iAIDetect++)
		{
			AIDetectGizmos iterDetect = m_AIDetectGizmos[iAIDetect];
			Gizmos.color = iterDetect.IsSafe ? Color.green : Color.red;
			Gizmos.DrawLine(iterDetect.StartPosition, iterDetect.EndPosition);
		}
	}
#endif

	protected override void HandleInitialize()
	{
		m_SafeAreaMinPosition = slWorld.GetInstance().GetMap().GetMapBounds().min
			+ new Vector2(slConstants.SNAKE_AIMOVEMENT_SAFEAREA_MAP_EDGE, slConstants.SNAKE_AIMOVEMENT_SAFEAREA_MAP_EDGE);
		m_SafeAreaMaxPosition = slWorld.GetInstance().GetMap().GetMapBounds().max
			- new Vector2(slConstants.SNAKE_AIMOVEMENT_SAFEAREA_MAP_EDGE, slConstants.SNAKE_AIMOVEMENT_SAFEAREA_MAP_EDGE);

		m_ClockwiseDetectAngle = Quaternion.Euler(0, 0, -slConstants.SNAKE_DETECT_ANGLE);
		m_CounterclockwiseDetectAngle = Quaternion.Euler(0, 0, slConstants.SNAKE_DETECT_ANGLE);
	}

	protected override void HandleUnController()
	{
#if UNITY_EDITOR
		m_AIDetectGizmos.Clear();
#endif
	}

	private void CalculateSafeArea(ref float moveDirection, float headPosition, float minPosition, float maxPosition)
	{
		if (headPosition < minPosition)
		{
			moveDirection = moveDirection < 0
				? -moveDirection
				: moveDirection;
		}
		else if (headPosition > maxPosition)
		{
			moveDirection = moveDirection > 0
				? -moveDirection
				: moveDirection;
		}
	}

#if UNITY_EDITOR
	private struct AIDetectGizmos
	{
		public Vector2 StartPosition;
		public Vector2 EndPosition;
		public bool IsSafe;
	}
#endif
}