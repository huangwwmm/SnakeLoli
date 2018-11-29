using UnityEngine;

public class slBaseController : MonoBehaviour 
{
	protected slSnake m_Snake;

	public void Initialize()
	{
		HandleInitialize();
	}

	public void SetControllerSnake(slSnake snake)
	{
		hwmDebug.Assert(m_Snake == null, "m_Snake == null");

		m_Snake = snake;

		HandleSetController();
	}

	public void UnControllerSnake()
	{
		hwmDebug.Assert(m_Snake != null, "m_Snake != null");

		HandleUnController();

		m_Snake = null;
	}

	public void Dispose()
	{
		HandleDispose();

		m_Snake = null;
	}

	public virtual bool IsAI()
	{
		return false;
	}

	public virtual bool IsPlayer()
	{
		return false;
	}

	protected virtual void HandleInitialize()
	{

	}

	protected virtual void HandleDispose()
	{

	}

	protected virtual void HandleSetController()
	{

	}

	protected virtual void HandleUnController()
	{

	}

	protected bool IsSafe(Vector2 moveDirection, float distance, bool ignorePredict)
	{
		RaycastHit2D[] hits = Physics2D.BoxCastAll((Vector2)m_Snake.GetHeadPosition() + moveDirection * distance * 0.5f
			, new Vector2(m_Snake.GetProperties().HeadColliderRadius * 1.2f, distance)
			, -Vector2.SignedAngle(moveDirection, Vector2.up)
			, Vector2.zero
			, Mathf.Infinity
			, ignorePredict 
				? (1 << (int)slConstants.Layer.Snake)
					| (1 << (int)slConstants.Layer.SnakeHead)
					| (1 << (int)slConstants.Layer.SnakePredict)
					| (1 << (int)slConstants.Layer.Wall)
				: (1 << (int)slConstants.Layer.Snake)
					| (1 << (int)slConstants.Layer.SnakeHead)
					| (1 << (int)slConstants.Layer.Wall));

		for (int iHit = 0; iHit < hits.Length; iHit++)
		{
			if (hits[iHit].collider.gameObject.name != m_Snake.GetGuidStr())
			{
				return false;
			}
			else if (hits[iHit].collider.gameObject.layer == (int)slConstants.Layer.Wall)
			{
				return false;
			}
		}
		return true;
	}

	protected bool IsHitPredict()
	{
		RaycastHit2D[] hits = Physics2D.CircleCastAll(m_Snake.GetHeadPosition()
			, m_Snake.GetProperties().HeadColliderRadius
			, Vector2.zero
			, Mathf.Infinity
			, (1 << (int)slConstants.Layer.SnakePredict));

		for (int iHit = 0; iHit < hits.Length; iHit++)
		{
			if (hits[iHit].collider.gameObject.name != m_Snake.GetGuidStr())
			{
				return true;
			}
		}
		return false;
	}
}