﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class slSnake : hwmActor
{
	public Vector2 TargetMoveDirection;

	public Action<slSkill.EventArgs> OnSpeedUpMovement;

	private Vector2 m_CurrentMoveDirection;

	private slSnakeTweakableProperties m_TweakableProperties;
	private slSnakeProperties m_Properties;
	private string m_SnakeName;

	private slBaseController m_Controller;

	private HeadNode m_Head;
	private ClothesNode m_Clothes;
	private hwmDeque<BodyNode> m_Bodys;
	private SpeedState m_SpeedState;
	private slSkill.EventArgs m_SkillEventArgs;
	private int m_EnableDamageLayers;
	private bool m_CanEatFood;

	private float m_Power;

	private bool m_IsReaminsFoodContamination;
	private float m_RemainsFoodContaminationPower;

	public float GetPower()
	{
		return m_Power;
	}

	public void CostPower(float power)
	{
		m_Power -= power;
	}

	public SpeedState GetSpeedState()
	{
		return m_SpeedState;
	}

	public void ChangeSpeedState(SpeedState speedState)
	{
		m_SpeedState = speedState;
	}

	public Vector3 GetHeadPosition()
	{
		return m_Head.Node.transform.localPosition;
	}

	public slSnakeTweakableProperties GetTweakableProperties()
	{
		return m_TweakableProperties;
	}

	public slSnakeProperties GetProperties()
	{
		return m_Properties;
	}

	public void DoUpdateMovement(float deltaTime)
	{
		int moveNodeCount = 0;
		switch (m_SpeedState)
		{
			case SpeedState.Normal:
				moveNodeCount = m_TweakableProperties.NormalMoveNodeCount;
				break;
			case SpeedState.SpeedUp:
				OnSpeedUpMovement(m_SkillEventArgs);
				if (m_SkillEventArgs.CanKeepSkill)
				{
					moveNodeCount = (int)m_SkillEventArgs.Effect;
					m_Power -= m_SkillEventArgs.CostPower;
				}
				else
				{
					moveNodeCount = m_TweakableProperties.NormalMoveNodeCount;
					m_SpeedState = SpeedState.Normal;
				}
				break;
			default:
				hwmDebug.Assert(false, "invalid SpeedState: " + m_SpeedState);
				break;
		}

		int bodyCountOffset = Mathf.Max(Mathf.FloorToInt(m_Power / m_TweakableProperties.NodeToPower - 2) // magic number: must have 2 node(head and chlothes)
				- m_Bodys.Count
			, -m_Bodys.Count);

		while (moveNodeCount-- > 0)
		{
			m_CurrentMoveDirection = hwmUtility.CircleLerp(m_CurrentMoveDirection, TargetMoveDirection, m_TweakableProperties.MaxTurnAngularSpeed * deltaTime);
			Quaternion headRotation = Quaternion.Euler(0, 0, -Vector2.SignedAngle(m_CurrentMoveDirection, Vector2.up));
			Vector3 lastNodePosition = m_Head.Node.transform.localPosition;
			Quaternion lastNodeRotation = m_Head.Node.transform.localRotation;
			m_Head.Node.transform.localPosition = m_Head.Node.transform.localPosition + headRotation * new Vector2(0, slConstants.SNAKE_NODE_TO_NODE_DISTANCE);
			m_Head.Node.transform.localRotation = headRotation;
			m_Head.Predict.transform.localPosition = m_Head.Node.transform.localPosition;
			m_Head.Predict.transform.localRotation = m_Head.Node.transform.localRotation;

			Vector3 swapNodePosition = m_Clothes.Node.transform.localPosition;
			Quaternion swapNodeRotation = m_Clothes.Node.transform.localRotation;
			m_Clothes.Node.transform.localPosition = lastNodePosition;
			m_Clothes.Node.transform.localRotation = lastNodeRotation;
			lastNodePosition = swapNodePosition;
			lastNodeRotation = swapNodeRotation;

			if (m_Bodys.Count > 0)
			{
				BodyNode newFrontNode;
				if (bodyCountOffset > 0)
				{
					bodyCountOffset--;
					newFrontNode = slWorld.GetInstance().GetSnakePool().PopBodyNode(m_SnakeName, m_GuidStr);
					newFrontNode.Collider.radius = m_Properties.BodyColliderRadius;
				}
				else
				{
					newFrontNode = m_Bodys.PopBack();
				}
				BodyNode oldFrontNode = m_Bodys.PeekFront();
				m_Bodys.PushFront(newFrontNode);
				newFrontNode.Node.transform.localPosition = lastNodePosition;
				newFrontNode.Node.transform.localRotation = lastNodeRotation;
				if (newFrontNode.Sprite != null)
				{
					newFrontNode.Sprite.sortingOrder = oldFrontNode.Sprite.sortingOrder + 1;
					if (newFrontNode.Sprite.sortingOrder >= slConstants.SNAKE_SPRITERENDERER_MAX_ORDERINLAYER)
					{
						ResetOrderInLayer();
					}
				}
			}
		}

		while (bodyCountOffset++ < 0)
		{
			BodyNode backNode = m_Bodys.PopBack();
			slWorld.GetInstance().GetSnakePool().PushBodyNode(m_SnakeName, backNode);
		}
	}

	public void DoUpdateEatFood()
	{
		EatFood(m_TweakableProperties.EatFoodRadius);
	}

	public slBaseController GetController()
	{
		return m_Controller;
	}

	public void EatFood(float radius)
	{
		Vector2 headPosition = m_Head.Node.transform.localPosition;
		hwmQuadtree<slFood>.BoundsEnumerator enumerator = new hwmQuadtree<slFood>.BoundsEnumerator(slWorld.GetInstance().GetFoodSystem().GetQuadtree().GetRootNode()
			, new hwmBounds2D(headPosition, new Vector2(radius * 2.0f, radius * 2.0f)));
		while (enumerator.MoveNext())
		{
			hwmQuadtree<slFood>.Node iterNode = enumerator.Current;
			hwmBetterList<slFood> foods = iterNode.GetElements();
			bool inCircleInside = iterNode.GetLooseBounds().InCircleInside(headPosition, radius);
			for (int iFood = foods.Count - 1; iFood >= 0; iFood--)
			{
				slFood iterFood = foods[iFood];
				if (inCircleInside
					|| ((Vector2)iterFood.transform.localPosition - headPosition).sqrMagnitude <= radius * radius)
				{
					EatFood(iterFood);
				}
			}
		}
	}

	public bool CanEatFood()
	{
		return m_CanEatFood;
	}

	public void EnableEatFood(bool enable)
	{
		m_CanEatFood = enable;
	}

	public void EnableDamageLayer(int layer, bool enable)
	{
		if (enable)
		{
			m_EnableDamageLayers |= 1 << layer;
		}
		else
		{
			m_EnableDamageLayers &= ~(1 << layer);
		}
	}

	public void EnableRemainsFoodContamination(bool enable, float foodPower)
	{
		m_IsReaminsFoodContamination = enable;
		m_RemainsFoodContaminationPower = foodPower;
	}

	protected override void HandleInitialize(object additionalData)
	{
		InitializeAdditionalData initializeData = additionalData as InitializeAdditionalData;
		hwmDebug.Assert(initializeData.NodeCount >= slConstants.SNAKE_INITIALIZEZ_NODE_MINCOUNT, "initializeData.NodeCount >= slConstants.SNAKE_INITIALIZEZ_NODE_MINCOUNT");

		m_SnakeName = initializeData.SnakeName;
		slWorld.GetInstance().GetSnakePool().LoadSnake(m_SnakeName);
		m_TweakableProperties = slWorld.GetInstance().GetSnakePool().GetTweakableProperties(initializeData.TweakableProperties);
		m_Properties = slWorld.GetInstance().GetSnakePool().GetProperties(m_SnakeName);

		m_Power = initializeData.NodeCount * m_TweakableProperties.NodeToPower;

		m_Head = slWorld.GetInstance().GetSnakePool().PopHeadNode(m_SnakeName, m_GuidStr);
		m_Head.Node.transform.localPosition = initializeData.HeadPosition;
		m_Head.Node.transform.localRotation = initializeData.HeadRotation;
		m_Head.Collider.radius = m_Properties.HeadColliderRadius;
		m_Head.Trigger.OnTriggerEnter += OnTrigger;
		m_Head.Predict.transform.localPosition = initializeData.HeadPosition;
		m_Head.Predict.transform.localRotation = initializeData.HeadRotation;
		m_Head.PredictCollider.size = new Vector2(m_Properties.HeadColliderRadius * slConstants.SNAKE_PREDICT_SIZE_X, slConstants.SNAKE_NODE_TO_NODE_DISTANCE * slConstants.SNAKE_PREDICT_SIZE_Y);
		m_Head.PredictCollider.offset = new Vector2(0, m_Head.PredictCollider.size.y * 0.5f + m_Properties.HeadColliderRadius);

		m_Clothes = slWorld.GetInstance().GetSnakePool().PopClothesNode(m_SnakeName, m_GuidStr);
		m_Clothes.Node.transform.localPosition = initializeData.HeadPosition + initializeData.HeadRotation * new Vector3(0, -slConstants.SNAKE_NODE_TO_NODE_DISTANCE, 0);
		m_Clothes.Node.transform.localRotation = initializeData.HeadRotation;
		m_Clothes.Collider.radius = m_Properties.ClothesColliderRadius;

		m_Bodys = new hwmDeque<BodyNode>(16);
		Vector3 bodyToClothesOffset = initializeData.HeadRotation * new Vector3(0, -slConstants.SNAKE_NODE_TO_NODE_DISTANCE, 0);
		Vector3 bodyToBodyOffset = initializeData.HeadRotation * new Vector3(0, -slConstants.SNAKE_NODE_TO_NODE_DISTANCE, 0);
		for (int iNode = 3; iNode <= initializeData.NodeCount; iNode++) // MagicNumber: 1->Head 2->Clothes 3->FirstBody
		{
			BodyNode node = slWorld.GetInstance().GetSnakePool().PopBodyNode(m_SnakeName, m_GuidStr);
			node.Node.transform.localPosition = m_Clothes.Node.transform.position + bodyToClothesOffset + bodyToBodyOffset * (iNode - 3);
			node.Node.transform.localRotation = initializeData.HeadRotation;
			node.Collider.radius = m_Properties.BodyColliderRadius;
			if (node.Sprite != null)
			{
				node.Sprite.sortingOrder = slConstants.SNAKE_SPRITERENDERER_MIN_ORDERINLAYER + initializeData.NodeCount - iNode;
			}

			m_Bodys.PushBack(node);
		}

		if (initializeData.IsBot)
		{
			m_Controller = gameObject.AddComponent<slAIController>();
			m_Controller.Initialize();
		}
		else
		{
			m_Controller = slWorld.GetInstance().GetPlayerController();
		}
		m_Controller.SetControllerSnake(this);

		TargetMoveDirection = (initializeData.HeadRotation * Vector2.up).normalized;
		m_CurrentMoveDirection = TargetMoveDirection;
		m_SpeedState = SpeedState.Normal;

		m_EnableDamageLayers = int.MaxValue;
		m_CanEatFood = true;

		m_SkillEventArgs = new slSkill.EventArgs();

		m_IsReaminsFoodContamination = false;
	}

	protected override void HandleDispose(object additionalData)
	{
		DisposeAdditionalData disposeAdditionalData = additionalData as DisposeAdditionalData;
		slFood.FoodType foodType = m_IsReaminsFoodContamination ? slFood.FoodType.Contamination : slFood.FoodType.Remains;
		float power = m_IsReaminsFoodContamination ? m_RemainsFoodContaminationPower : m_TweakableProperties.RemainsFoodPower;
		if (disposeAdditionalData.MyDeadType != DeadType.FinishGame)
		{
			slWorld.GetInstance().GetFoodSystem().AddCreateEvent(foodType, m_Head.Node.transform.position, m_Properties.DeadFoodColor, power);
			slWorld.GetInstance().GetFoodSystem().AddCreateEvent(foodType, m_Clothes.Node.transform.position, m_Properties.DeadFoodColor, power);
			foreach (BodyNode node in m_Bodys)
			{
				slWorld.GetInstance().GetFoodSystem().AddCreateEvent(foodType, node.Node.transform.position, m_Properties.DeadFoodColor, power);
			}
		}

		m_SkillEventArgs = null;

		m_Controller.UnControllerSnake();
		m_Controller = null;

		m_Head.Trigger.OnTriggerEnter -= OnTrigger;
		slWorld.GetInstance().GetSnakePool().PushHeadNode(m_SnakeName, m_Head);
		m_Head = null;

		slWorld.GetInstance().GetSnakePool().PushClothesNode(m_SnakeName, m_Clothes);
		m_Clothes = null;

		int bodyCount = m_Bodys.Count;
		for (int iBody = 0; iBody < bodyCount; iBody++)
		{
			BodyNode bodyNode = m_Bodys.PopBack();
			slWorld.GetInstance().GetSnakePool().PushBodyNode(m_SnakeName, bodyNode);
		}
		m_Bodys.Clear();
		m_Bodys = null;

		Destroy(m_TweakableProperties);
		m_TweakableProperties = null;

		Destroy(m_TweakableProperties);
		m_Properties = null;
	}

	protected void OnDrawGizmos()
	{
		Gizmos.color = Color.black;
		Gizmos.DrawLine(m_Head.Node.transform.localPosition, m_Head.Node.transform.localPosition + (Vector3)TargetMoveDirection * slConstants.SNAKE_DETECT_DISTANCE);
	}

	private T CreateNode<T>(string name, GameObject presentation, float colliderRadius, Vector3 position, Quaternion rotation) where T : BodyNode, new()
	{
		T node = new T();
		bool isHead = node is HeadNode;

		node.Node.transform.localPosition = position;
		node.Node.transform.localRotation = rotation;

		return node;
	}

	private void OnTrigger(Collider2D collider)
	{
		switch (collider.gameObject.layer)
		{
			// dead
			case (int)slConstants.Layer.Wall:
			case (int)slConstants.Layer.Snake:
			case (int)slConstants.Layer.SnakeHead:
				int colliderLayer = collider.gameObject.layer;
				if (((1 << colliderLayer) & m_EnableDamageLayers) != 0)
				{
					DisposeAdditionalData disposeAdditionalData = new DisposeAdditionalData();
					disposeAdditionalData.MyDeadType = collider.gameObject.layer == (int)slConstants.Layer.Wall
						? DeadType.HitWall
						: DeadType.HitSnake;
					hwmWorld.GetInstance().DestroyActor(this, disposeAdditionalData);
				}
				break;
		}
	}

	private void ResetOrderInLayer()
	{
		int currentOrder = slConstants.SNAKE_SPRITERENDERER_MIN_ORDERINLAYER + m_Bodys.Count;
		foreach (BodyNode body in m_Bodys)
		{
			body.Sprite.sortingOrder = currentOrder--;
		}
	}

	private void EatFood(slFood food)
	{
		if (m_CanEatFood)
		{
			m_Power += food.BeEat(m_Head.Node.transform);
		}
	}

	[Serializable]
	public class InitializeAdditionalData
	{
		public string SnakeName;
		public int NodeCount;
		public bool IsBot;
		public Vector3 HeadPosition = Vector3.zero;
		public Quaternion HeadRotation = Quaternion.identity;
		public string TweakableProperties;
	}

	[Serializable]
	public class DisposeAdditionalData
	{
		public DeadType MyDeadType;
	}

	public class HeadNode : BodyNode
	{
		public new SpriteRenderer[] Sprite;
		public slSnakeHeadTrigger Trigger;
		public Rigidbody2D Rigidbody;
		public GameObject Predict;
		public BoxCollider2D PredictCollider;
	}

	public class ClothesNode : BodyNode
	{
	}

	public class BodyNode
	{
		public GameObject Node;
		public CircleCollider2D Collider;
		public SpriteRenderer Sprite;
	}

	public enum DeadType
	{
		HitWall,
		HitSnake,
		FinishGame
	}

	public enum SpeedState
	{
		Normal,
		SpeedUp,
	}
}