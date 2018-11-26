﻿using UnityEngine;

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
		RaycastHit2D hit = Physics2D.BoxCast((Vector2)m_Snake.GetHeadPosition() + moveDirection * distance * 0.5f
			, new Vector2(m_Snake.MyProperties.HeadColliderRadius * 1.2f, distance)
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

		return hit.collider == null;
	}

	protected bool IsHitPredict()
	{
		RaycastHit2D hit = Physics2D.CircleCast(m_Snake.GetHeadPosition()
		, m_Snake.MyProperties.HeadColliderRadius
		, Vector2.zero
		, Mathf.Infinity
		, (1 << (int)slConstants.Layer.SnakePredict));

		return hit.collider == null;
	}
}