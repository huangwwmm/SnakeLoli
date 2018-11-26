using UnityEngine;

public class slAIController : slBaseController
{
	public override bool IsAI()
	{
		return true;
	}

	public void DoAIUpdate()
	{
		Vector2 targetDirection = m_Snake.GetCurrentMoveDirection();
		Vector2 headPosition = m_Snake.GetHeadPosition();
		slWorld.GetInstance().GetAISystem().CalculatieSnakeMoveDirectionInSafeArea(ref targetDirection, headPosition);

		m_Snake.TargetMoveDirection = targetDirection;
	}
}