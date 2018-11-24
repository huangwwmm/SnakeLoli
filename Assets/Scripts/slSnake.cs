using System;
using System.Collections.Generic;
using UnityEngine;

public class slSnake : hwmActor
{
	public Properties MyProperties;

	public Vector2 MoveDirection;

	private Vector2 m_CurrentMoveDirection;

	private slSnakePresentation m_Presentation;
	private slSnakeTweakableProperties m_TweakableProperties;

	private HeadNode m_Head;
	private ClothesNode m_Clothes;
	private hwmDeque<BodyNode> m_Bodys;

	private float m_CumulativeMoveDistance;
	private float m_CumulativeMoveTime;

	public Vector3 GetHeadPosition()
	{
		return m_Head.Node.transform.localPosition;
	}

	protected override void HandleInitialize(object additionalData)
	{
		InitializeAdditionalData initializeData = additionalData as InitializeAdditionalData;
		hwmDebug.Assert(initializeData.NodeCount >= slConstants.SNAKE_INITIALIZEZ_NODE_MINCOUNT, "initializeData.NodeCount >= slConstants.SNAKE_INITIALIZEZ_NODE_MINCOUNT");

		m_TweakableProperties = Instantiate(hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.SnakeTweakableProperties
					, initializeData.TweakableProperties)) as slSnakeTweakableProperties;

		m_CumulativeMoveDistance = 0;

		if (slWorld.GetInstance().NeedPresentation())
		{
			m_Presentation = (Instantiate(hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Actor
					, "SnakePresentation_" + MyProperties.SnakeName)) as GameObject)
				.GetComponent<slSnakePresentation>();
			m_Presentation.gameObject.transform.SetParent(transform);
		}

		m_Head = CreateNode<HeadNode>("Head"
			, m_Presentation != null ? m_Presentation.MyProperties.Head : null
			, MyProperties.HeadColliderRadius
			, initializeData.HeadPosition
			, initializeData.HeadRotation);

		m_Clothes = CreateNode<ClothesNode>("Clothes"
			, m_Presentation != null ? m_Presentation.MyProperties.Clothes : null
			, MyProperties.ClothesColliderRadius
			, initializeData.HeadPosition + initializeData.HeadRotation * new Vector3(0, -MyProperties.NodeToNodeDistance, 0)
			, initializeData.HeadRotation);

		m_Bodys = new hwmDeque<BodyNode>(16);
		Vector3 bodyToClothesOffset = initializeData.HeadRotation * new Vector3(0, -MyProperties.NodeToNodeDistance, 0);
		Vector3 bodyToBodyOffset = initializeData.HeadRotation * new Vector3(0, -MyProperties.NodeToNodeDistance, 0);
		for (int iNode = 3; iNode <= initializeData.NodeCount; iNode++) // MagicNumber: 1->Head 2->Clothes 3->FirstBody
		{
			BodyNode node = CreateNode<BodyNode>("Body"
				, m_Presentation != null ? m_Presentation.MyProperties.Body : null
				, MyProperties.BodyColliderRadius
				, m_Clothes.Node.transform.position + bodyToClothesOffset + bodyToBodyOffset * (iNode - 3)
				, initializeData.HeadRotation);
			node.Sprite.sortingOrder = slConstants.SNAKE_SPRITERENDERER_MIN_ORDERINLAYER + initializeData.NodeCount - iNode;

			m_Bodys.PushBack(node);
		}

		slBaseController controller;
		if (initializeData.IsBot)
		{
			controller = gameObject.AddComponent<slAIController>();
			controller.Initialize();
		}
		else
		{
			controller = slWorld.GetInstance().GetPlayerController();
		}
		controller.SetControllerSnake(this);

		MoveDirection = (initializeData.HeadRotation * Vector2.up).normalized;
		m_CurrentMoveDirection = MoveDirection;
		m_CumulativeMoveTime = 0;
	}

	protected override void HandleDispose(object additionalData)
	{
		DisposeAdditionalData disposeAdditionalData = additionalData as DisposeAdditionalData;
		if (disposeAdditionalData.MyDeadType != DeadType.FinishGame)
		{
			slWorld.GetInstance().GetFoodSystem().AddCreateEvent(slFood.FoodType.Large, m_Head.Node.transform.position, MyProperties.DeadFoodColor);
			slWorld.GetInstance().GetFoodSystem().AddCreateEvent(slFood.FoodType.Large, m_Clothes.Node.transform.position, MyProperties.DeadFoodColor);
			foreach(BodyNode node in m_Bodys)
			{
				slWorld.GetInstance().GetFoodSystem().AddCreateEvent(slFood.FoodType.Large, node.Node.transform.position, MyProperties.DeadFoodColor);
			}
		}

		m_Head.Trigger.OnTriggerEnter -= OnTrigger;

		m_Bodys.Clear();
		m_Bodys = null;

		Destroy(m_TweakableProperties);
		m_TweakableProperties = null;

		if (m_Presentation != null)
		{
			Destroy(m_Presentation.gameObject);
			m_Presentation = null;
		}
	}

	protected void Update()
	{
		float deltaTime = Time.deltaTime;
		UpdateMovement(deltaTime);
	}

	private void UpdateMovement(float deltaTime)
	{
		m_CumulativeMoveDistance += deltaTime * m_TweakableProperties.NormalSpeed;
		m_CumulativeMoveTime += deltaTime;

		int moveCount = 0;
		while (m_CumulativeMoveDistance >= MyProperties.NodeToNodeDistance)
		{
			m_CurrentMoveDirection = hwmUtility.CircleLerp(m_CurrentMoveDirection, MoveDirection, m_TweakableProperties.MaxTurnAngularSpeed * m_CumulativeMoveTime).normalized;
			m_CumulativeMoveDistance -= MyProperties.NodeToNodeDistance;
			m_CumulativeMoveTime = 0;
			moveCount++;
		}

		while (moveCount-- > 0)
		{
			Quaternion headRotation = Quaternion.Euler(0, 0, -Vector2.SignedAngle(m_CurrentMoveDirection, Vector2.up));
			Vector3 lastNodePosition = m_Head.Node.transform.localPosition;
			Quaternion lastNodeRotation = m_Head.Node.transform.localRotation;
			m_Head.Node.transform.localPosition = m_Head.Node.transform.localPosition + headRotation * new Vector2(0, MyProperties.NodeToNodeDistance);
			m_Head.Node.transform.localRotation = headRotation;

			Vector3 swapNodePosition = m_Clothes.Node.transform.localPosition;
			Quaternion swapNodeRotation = m_Clothes.Node.transform.localRotation;
			m_Clothes.Node.transform.localPosition = lastNodePosition;
			m_Clothes.Node.transform.localRotation = lastNodeRotation;
			lastNodePosition = swapNodePosition;
			lastNodeRotation = swapNodeRotation;

			if (m_Bodys.Count > 0)
			{
				BodyNode backNode = m_Bodys.PopBack();
				BodyNode frontNode = m_Bodys.PeekFront();
				backNode.Sprite.sortingOrder = frontNode.Sprite.sortingOrder + 1;
				backNode.Node.transform.localPosition = lastNodePosition;
				backNode.Node.transform.localRotation = lastNodeRotation;
				m_Bodys.PushFront(backNode);

				if (backNode.Sprite.sortingOrder >= slConstants.SNAKE_SPRITERENDERER_MAX_ORDERINLAYER)
				{
					ResetOrderInLayer();
				}
			}
		}
	}

	private T CreateNode<T>(string name, GameObject presentation, float colliderRadius, Vector3 position, Quaternion rotation) where T : BodyNode, new()
	{
		T node = new T();
		bool isHead = node is HeadNode;
		bool isClothes = !isHead && node is ClothesNode;
		bool isBody = !isHead && !isClothes;

		node.Node = presentation != null
			? Instantiate(presentation) : new GameObject();
		node.Node.name = name;
		node.Node.transform.SetParent(transform);
		node.Node.transform.localPosition = position;
		node.Node.transform.localRotation = rotation;

		node.Collider = node.Node.AddComponent<CircleCollider2D>();
		node.Collider.radius = colliderRadius;
		node.Collider.isTrigger = isHead;

		if (isHead)
		{
			HeadNode headNode = node as HeadNode;
			headNode.Trigger = headNode.Node.AddComponent<slSnakeHeadTrigger>();
			headNode.Rigidbody = headNode.Node.AddComponent<Rigidbody2D>();
			headNode.Rigidbody.isKinematic = true;
			headNode.Trigger.OnTriggerEnter += OnTrigger;
		}

		if (presentation != null)
		{
			if (isHead)
			{
				(node as HeadNode).Sprite = node.Node.GetComponentsInChildren<SpriteRenderer>();
			}
			else
			{
				node.Sprite = node.Node.GetComponent<SpriteRenderer>();
			}
		}
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
				DisposeAdditionalData disposeAdditionalData = new DisposeAdditionalData();
				disposeAdditionalData.MyDeadType = collider.gameObject.layer == (int)slConstants.Layer.Wall
					? DeadType.HitWall
					: DeadType.HitSnake;
				hwmWorld.GetInstance().DestroyActor(this, disposeAdditionalData);
				break;
			case (int)slConstants.Layer.Food:
				slFood food = collider.gameObject.GetComponent<slFood>();
				food.BeEat(m_Head.Node.transform);
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

	[System.Serializable]
	public class Properties
	{
		public string SnakeName;

		public float HeadColliderRadius;
		public float ClothesColliderRadius;
		public float BodyColliderRadius;

		public float NodeToNodeDistance;

		public Color DeadFoodColor;
	}

	[System.Serializable]
	public class InitializeAdditionalData
	{
		public int NodeCount;
		public bool IsBot;
		public Vector3 HeadPosition = Vector3.zero;
		public Quaternion HeadRotation = Quaternion.identity;
		public string TweakableProperties;
	}

	[System.Serializable]
	public class DisposeAdditionalData
	{
		public DeadType MyDeadType;
	}

	public class HeadNode : BodyNode
	{
		public new SpriteRenderer[] Sprite;
		public slSnakeHeadTrigger Trigger;
		public Rigidbody2D Rigidbody;
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
}