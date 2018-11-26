using UnityEngine;

public class slAIController : slBaseController
{
	private Vector2 m_SafeAreaMinPosition;
	private Vector2 m_SafeAreaMaxPosition;
	private int m_NotChangeDirectionTimes = 0;

	public override bool IsAI()
	{
		return true;
	}

	public void DoAIUpdate()
	{
		m_Snake.SetColliderEnableForAI(false);
		Vector2 targetDirection = m_Snake.TargetMoveDirection;

		if (m_NotChangeDirectionTimes >= slConstants.SNAKE_RANDMOVEMOENT_WHENNOTCHANGED_TIMES
			&& hwmRandom.RandFloat() < slConstants.SNAKE_RANDMOVEMOENT_WHENNOTCHANGED_PROBABILITY)
		{
			targetDirection = hwmRandom.RandVector2(-Vector2.one, Vector2.one);
		}

		CalculateSafeArea(ref targetDirection.x, m_Snake.GetHeadPosition().x, m_SafeAreaMinPosition.x, m_SafeAreaMaxPosition.x);
		CalculateSafeArea(ref targetDirection.y, m_Snake.GetHeadPosition().y, m_SafeAreaMinPosition.y, m_SafeAreaMaxPosition.y);

		if (!IsSafe(targetDirection, slConstants.SNAKE_DETECT_DISTANCE, false))
		{
			Quaternion angle = Quaternion.Euler(0, 0
				, hwmRandom.RandFloat() > 0.5f
					? -slConstants.SNAKE_DETECT_ANGLE
					: slConstants.SNAKE_DETECT_ANGLE);
			float totalAngle = 0;
			Vector2 currentCalculateDirection = targetDirection;
			bool ignorePredict = IsHitPredict();
			while (true)
			{
				currentCalculateDirection = angle * currentCalculateDirection;
				totalAngle += slConstants.SNAKE_DETECT_ANGLE;
				if (IsSafe(currentCalculateDirection, slConstants.SNAKE_DETECT_DISTANCE, ignorePredict))
				{
					targetDirection = currentCalculateDirection;
					break;
				}

				if (totalAngle > 360)
				{
					break;
				}
			}
		}

		m_NotChangeDirectionTimes = m_Snake.TargetMoveDirection == targetDirection.normalized 
			? m_NotChangeDirectionTimes + 1 : 0;
		m_Snake.TargetMoveDirection = targetDirection.normalized;
		m_Snake.SetColliderEnableForAI(true);
	}

	protected override void HandleInitialize()
	{
		m_SafeAreaMinPosition = slWorld.GetInstance().GetMap().GetMapBounds().min
			+ new Vector2(slConstants.SNAKE_AIMOVEMENT_SAFEAREA_MAP_EDGE, slConstants.SNAKE_AIMOVEMENT_SAFEAREA_MAP_EDGE);
		m_SafeAreaMaxPosition = slWorld.GetInstance().GetMap().GetMapBounds().max
			- new Vector2(slConstants.SNAKE_AIMOVEMENT_SAFEAREA_MAP_EDGE, slConstants.SNAKE_AIMOVEMENT_SAFEAREA_MAP_EDGE);
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