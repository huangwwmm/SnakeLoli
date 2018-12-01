﻿using System.Collections.Generic;
using UnityEngine;

public class slBaseController : MonoBehaviour 
{
	protected slSnake m_Snake;

	public slSnake GetControllerSnake()
	{
		return m_Snake;
	}

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

	protected DangerType IsSafe(hwmQuadtree<slSnake.QuadtreeElement>.AABBEnumerator enumerator, Vector2 moveDirection, float distance, bool ignorePredict)
	{
		Vector2 start = m_Snake.GetHeadPosition();
		Vector2 end = start + moveDirection * distance;
		if (!slWorld.GetInstance().GetMap().GetMapBox().IsInsideOrOn(end))
		{
			return DangerType.Wall;
		}

		hwmSphere2D headSphere = new hwmSphere2D(m_Snake.GetHeadPosition(), m_Snake.GetHeadRadius());

		enumerator.Reset();
		while (enumerator.MoveNext())
		{
			hwmQuadtree<slSnake.QuadtreeElement>.Node iterNode = enumerator.Current;
			hwmBetterList<slSnake.QuadtreeElement> elements = iterNode.GetElements();
			for (int iElement = elements.Count - 1; iElement >= 0; iElement--)
			{
				slSnake.QuadtreeElement iterElement = elements[iElement];
				hwmDebug.Assert(iterElement.Owner != null, "VaildNode");
				if (iterElement.Owner != m_Snake)
				{
					if (iterElement.NodeType != slConstants.NodeType.Predict)
					{
						if (iterElement.AABB.LineIntersection(start, end))
						{
							return DangerType.Snake;
						}
					}
					else if (!ignorePredict)
					{
						Quaternion rotation = Quaternion.Inverse(iterElement.GetRotation());
						Vector2 offset = (Vector2)iterElement.GetPosition() - hwmUtility.QuaternionMultiplyVector(rotation, iterElement.GetPosition());
						start = hwmUtility.QuaternionMultiplyVector(rotation, start) + offset;
						end = hwmUtility.QuaternionMultiplyVector(rotation, end) + offset;
						if (((slSnake.PredictNode)iterElement).Box.LineIntersection(start, end))
						{
							return DangerType.Predict;
						}
					}
				}
			}
		}
		return DangerType.Safe;
	}

	protected bool IsHitPredict()
	{
		float radius = m_Snake.GetHeadRadius();
		hwmBox2D collideAABB = hwmBox2D.BuildAABB(m_Snake.GetHeadPosition(), new Vector2(radius, radius));
		hwmQuadtree<slSnake.QuadtreeElement>.AABBEnumerator enumerator = new hwmQuadtree<slSnake.QuadtreeElement>.AABBEnumerator(
			slWorld.GetInstance().GetSnakeSystem().GetQuadtree().GetRootNode(), collideAABB);

		while (enumerator.MoveNext())
		{
			hwmQuadtree<slSnake.QuadtreeElement>.Node iterNode = enumerator.Current;
			hwmBetterList<slSnake.QuadtreeElement> elements = iterNode.GetElements();
			for (int iElement = elements.Count - 1; iElement >= 0; iElement--)
			{
				slSnake.QuadtreeElement iterElement = elements[iElement];
				if (iterElement.Owner != m_Snake
					&& iterElement.NodeType == slConstants.NodeType.Predict)
				{
					Vector2 predictCenter = iterElement.GetPosition();
					Quaternion predictRotationInverse = Quaternion.Inverse(iterElement.GetRotation());
					Vector2 sphereCenter = hwmUtility.QuaternionMultiplyVector(predictRotationInverse, m_Snake.GetHeadPosition()) 
						+ predictCenter
						- hwmUtility.QuaternionMultiplyVector(predictRotationInverse, predictCenter);
					if (((slSnake.PredictNode)iterElement).Box.IntersectSphere(sphereCenter, radius * radius))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public enum DangerType
	{
		Safe,
		Wall,
		Snake,
		Predict,
	}
}