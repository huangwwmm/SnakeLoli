﻿using System.Collections.Generic;
using UnityEngine;

public class slBaseController : MonoBehaviour 
{
	protected List<hwmQuadtree<slSnake.QuadtreeElement>.Node> ms_SnakeQuadtreeNodes = new List<hwmQuadtree<slSnake.QuadtreeElement>.Node>();

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

		HandleSetControllerSnake();
	}

	public void UnControllerSnake()
	{
		hwmDebug.Assert(m_Snake != null, "m_Snake != null");

		HandleUnControllerSnake();

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

	protected virtual void HandleSetControllerSnake()
	{

	}

	protected virtual void HandleUnControllerSnake()
	{

	}

	protected bool IsSafe(Vector2 moveDirection, float distance, bool ignorePredict)
	{
		Vector2 start = m_Snake.GetHeadPosition();
		Vector2 end = start + moveDirection * distance;
		Vector2 direction = end - start;
		Vector2 directionReciprocal = hwmMath.Reciprocal(direction);
		if (!slWorld.GetInstance().GetMap().GetMapBox().IsInsideOrOn(end))
		{
			return false;
		}

		for (int iNode = 0; iNode < ms_SnakeQuadtreeNodes.Count; iNode++)
		{
			hwmQuadtree<slSnake.QuadtreeElement>.Node iterNode = ms_SnakeQuadtreeNodes[iNode];
			hwmBetterList<slSnake.QuadtreeElement> elements = iterNode.GetElements();
			for (int iElement = elements.Count - 1; iElement >= 0; iElement--)
			{
				slSnake.QuadtreeElement iterElement = elements[iElement];
				if (iterElement.Owner != m_Snake.GetGuid())
				{
					if (iterElement.NodeType != slConstants.NodeType.Predict)
					{
						if (iterElement.AABB.LineIntersection(start, end, direction, directionReciprocal))
						{
							return false;
						}
					}
					else if (!ignorePredict)
					{
						Quaternion rotation = Quaternion.Inverse(iterElement.GetRotation());
						Vector2 center = iterElement.GetPosition();
						start = hwmMath.QuaternionMultiplyVector(rotation, start - center);
						end = hwmMath.QuaternionMultiplyVector(rotation, end - center);
						if (((slSnake.PredictNode)iterElement).Box.LineIntersection(start, end))
						{
							return false;
						}
					}
				}
			}
		}
		return true;
	}

	protected bool IsHitPredict()
	{
		ms_SnakeQuadtreeNodes.Clear();
		slWorld.GetInstance().GetSnakeSystem().GetQuadtree().GetRootNode()
			.GetAllIntersectNode(ref ms_SnakeQuadtreeNodes, hwmBox2D.BuildAABB(m_Snake.GetHeadPosition(), new Vector2(m_Snake.GetHeadRadius(), m_Snake.GetHeadRadius())));

		for (int iNode = 0; iNode < ms_SnakeQuadtreeNodes.Count; iNode++)
		{
			hwmQuadtree<slSnake.QuadtreeElement>.Node iterNode = ms_SnakeQuadtreeNodes[iNode];
			hwmBetterList<slSnake.QuadtreeElement> elements = iterNode.GetElements();
			for (int iElement = elements.Count - 1; iElement >= 0; iElement--)
			{
				slSnake.QuadtreeElement iterElement = elements[iElement];
				if (iterElement.Owner != m_Snake.GetGuid()
					&& iterElement.NodeType == slConstants.NodeType.Predict)
				{
					Vector2 predictCenter = iterElement.GetPosition();
					Quaternion predictRotationInverse = Quaternion.Inverse(iterElement.GetRotation());
					Vector2 sphereCenter = predictCenter 
						+ hwmMath.QuaternionMultiplyVector(predictRotationInverse, (Vector2)m_Snake.GetHeadPosition() - predictCenter);
					if (((slSnake.PredictNode)iterElement).Box.IntersectSphere(sphereCenter, m_Snake.GetHeadRadius() * m_Snake.GetHeadRadius()))
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}