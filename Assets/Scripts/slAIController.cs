using UnityEngine;

public class slAIController : slBaseController
{
	private Vector2 m_SafeAreaMinPosition;
	private Vector2 m_SafeAreaMaxPosition;
	private int m_NotChangeDirectionTimes = 0;
	private Quaternion m_ClockwiseDetectAngle;
	private Quaternion m_CounterclockwiseDetectAngle;

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
				targetDirection = hwmRandom.RandVector2(-Vector2.one, Vector2.one);
			}
			m_NotChangeDirectionTimes = -1;
		}

		CalculateSafeArea(ref targetDirection.x, m_Snake.GetHeadPosition().x, m_SafeAreaMinPosition.x, m_SafeAreaMaxPosition.x);
		CalculateSafeArea(ref targetDirection.y, m_Snake.GetHeadPosition().y, m_SafeAreaMinPosition.y, m_SafeAreaMaxPosition.y);

		if (!IsSafe(targetDirection, slConstants.SNAKE_DETECT_DISTANCE, false)
			|| !IsSafe(m_ClockwiseDetectAngle * targetDirection, slConstants.SNAKE_DETECT_DISTANCE, false)
			|| !IsSafe(m_CounterclockwiseDetectAngle * targetDirection, slConstants.SNAKE_DETECT_DISTANCE, false))
		{
			Quaternion angle = hwmRandom.RandFloat() > 0.5f
				? m_ClockwiseDetectAngle
				: m_CounterclockwiseDetectAngle;
			Vector2 currentCalculateDirection = Quaternion.Euler(0, 0, hwmRandom.RandFloat(-slConstants.SNAKE_BEGIN_DETECT_RAND_MAXANGLE, slConstants.SNAKE_BEGIN_DETECT_RAND_MAXANGLE))
				* targetDirection;
			bool ignorePredict = IsHitPredict();
			for (int iCalculate = 0; iCalculate < slConstants.SNAKE_AI_CALCULATE_TIMES; iCalculate++)
			{
				currentCalculateDirection = angle * currentCalculateDirection;
				if (IsSafe(currentCalculateDirection, slConstants.SNAKE_DETECT_DISTANCE, ignorePredict))
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

	protected override void HandleInitialize()
	{
		m_SafeAreaMinPosition = slWorld.GetInstance().GetMap().GetMapBounds().min
			+ new Vector2(slConstants.SNAKE_AIMOVEMENT_SAFEAREA_MAP_EDGE, slConstants.SNAKE_AIMOVEMENT_SAFEAREA_MAP_EDGE);
		m_SafeAreaMaxPosition = slWorld.GetInstance().GetMap().GetMapBounds().max
			- new Vector2(slConstants.SNAKE_AIMOVEMENT_SAFEAREA_MAP_EDGE, slConstants.SNAKE_AIMOVEMENT_SAFEAREA_MAP_EDGE);

		m_ClockwiseDetectAngle = Quaternion.Euler(0, 0, -slConstants.SNAKE_DETECT_ANGLE);
		m_CounterclockwiseDetectAngle = Quaternion.Euler(0, 0, slConstants.SNAKE_DETECT_ANGLE);
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
}