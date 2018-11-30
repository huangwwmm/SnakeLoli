﻿#if UNITY_EDITOR
using System;
using UnityEngine;

public class slQuadtreeGizmos : MonoBehaviour
{
	public static hwmQuadtree<slFood> FoodQuadtree;
	public static hwmQuadtree<slSnake.QuadtreeElement> SnakeQuadtree;

	public Action OnBehaviourDrawGizmos;

	protected void OnDrawGizmos()
	{
		if (OnBehaviourDrawGizmos != null)
		{
			OnBehaviourDrawGizmos();
		}
	}
}
#endif