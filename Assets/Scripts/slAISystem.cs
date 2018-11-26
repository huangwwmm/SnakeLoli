using UnityEngine;

public class slAISystem : MonoBehaviour
{
	private Vector2 m_SnakeMoveSafeAreaMinPosition;
	private Vector2 m_SnakeMoveSafeAreaMaxPosition;

	public void CalculatieSnakeMoveDirectionInSafeArea(ref Vector2 moveDirection, Vector2 headPosition)
	{
		CalculatieSnakeMoveDirectionInSafeArea(ref moveDirection.x, headPosition.x, m_SnakeMoveSafeAreaMinPosition.x, m_SnakeMoveSafeAreaMaxPosition.x);
		CalculatieSnakeMoveDirectionInSafeArea(ref moveDirection.y, headPosition.y, m_SnakeMoveSafeAreaMinPosition.y, m_SnakeMoveSafeAreaMaxPosition.y);
	}

	protected void Awake()
	{
		m_SnakeMoveSafeAreaMinPosition = slWorld.GetInstance().GetMap().GetMapBounds().min + new Vector2(slConstants.SNAKE_AIMOVEMENT_SAFEAREA_MAP_EDGE, slConstants.SNAKE_AIMOVEMENT_SAFEAREA_MAP_EDGE);
		m_SnakeMoveSafeAreaMaxPosition = slWorld.GetInstance().GetMap().GetMapBounds().max - new Vector2(slConstants.SNAKE_AIMOVEMENT_SAFEAREA_MAP_EDGE, slConstants.SNAKE_AIMOVEMENT_SAFEAREA_MAP_EDGE);
	}

	private void CalculatieSnakeMoveDirectionInSafeArea(ref float moveDirection, float headPosition, float minPosition, float maxPosition)
	{
		if (headPosition < minPosition)
		{
			moveDirection = moveDirection < 0
				? -moveDirection
				: moveDirection == 0
					? hwmRandom.RandFloat(0.1f, 1.0f)
					: moveDirection;
		}
		else if (headPosition > maxPosition)
		{
			moveDirection = moveDirection > 0
				? -moveDirection
				: moveDirection == 0
					? hwmRandom.RandFloat(0.1f, 1.0f)
					: moveDirection;
		}
	}
}