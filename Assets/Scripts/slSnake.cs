using System.Collections.Generic;
using UnityEngine;

public class slSnake : hwmActor
{
	public Properties MyProperties;

	public Vector2 TargetMoveDirection;

	private Vector2 m_CurrentMoveDirection;

	private slSnakePresentation m_Presentation;
	private slSnakeTweakableProperties m_TweakableProperties;

	private slBaseController m_Controller;

	private HeadNode m_Head;
	private ClothesNode m_Clothes;
	private hwmDeque<BodyNode> m_Bodys;
	private SpeedState m_SpeedState;

	private int m_Power;

	public Vector3 GetHeadPosition()
	{
		return m_Head.Node.transform.localPosition;
	}

	public void DoUpdateMovement(int moveTimes, float deltaTime)
	{
		int bodyCountOffset = (m_Power / m_TweakableProperties.PowerToNode - 2)
			- m_Bodys.Count;

		int moveNodeCountPreTime = 0;
		switch(m_SpeedState)
		{
			case SpeedState.Normal:
				moveNodeCountPreTime = m_TweakableProperties.NormalMoveNodeCount;
				break;
			case SpeedState.SpeedUp:
				moveNodeCountPreTime = m_TweakableProperties.SpeedUpMoveNodeCount;
				m_Power -= moveTimes * m_TweakableProperties.SpeedUpCostPower;
				if (!CanKeepSpeedUp())
				{
					m_SpeedState = SpeedState.Normal;
				}
				break;
			default:
				hwmDebug.Assert(false, "invalid SpeedState: " + m_SpeedState);
				break;
		}

		int moveNodeCount = moveNodeCountPreTime * moveTimes;
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
					newFrontNode = CreateNode<BodyNode>("Body"
						, m_Presentation != null ? m_Presentation.MyProperties.Body : null
						, MyProperties.BodyColliderRadius
						, slConstants.SNAKE_BODYNODE_CREATE_POSITION
						, Quaternion.identity);
				}
				else
				{
					newFrontNode = m_Bodys.PopBack();
				}
				BodyNode oldFrontNode = m_Bodys.PeekFront();
				newFrontNode.Sprite.sortingOrder = oldFrontNode.Sprite.sortingOrder + 1;
				newFrontNode.Node.transform.localPosition = lastNodePosition;
				newFrontNode.Node.transform.localRotation = lastNodeRotation;
				m_Bodys.PushFront(newFrontNode);

				if (newFrontNode.Sprite.sortingOrder >= slConstants.SNAKE_SPRITERENDERER_MAX_ORDERINLAYER)
				{
					ResetOrderInLayer();
				}
			}
		}

		while (bodyCountOffset++ < 0)
		{
			BodyNode backNode = m_Bodys.PopBack();
			Destroy(backNode.Node);
		}
	}

	public slBaseController GetController()
	{
		return m_Controller;
	}

	public bool CanSpeedUp()
	{
		return m_SpeedState == SpeedState.Normal
			? CanEnterSpeedUp()
			: CanKeepSpeedUp();
	}

	public void TryChangeSpeedState(SpeedState speedState)
	{
		if (m_SpeedState == SpeedState.SpeedUp
			&& !CanSpeedUp())
		{
			m_SpeedState = SpeedState.Normal;
		}
		m_SpeedState = speedState;
	}

	public void TestSkill()
	{
		hwmQuadtree<slFood> quadtree = slWorld.GetInstance().GetFoodSystem().GetQuadtree();
		hwmQuadtree<slFood>.Node targetNode = null;
		if (!quadtree.TryFindNode(ref targetNode, new hwmBounds2D(m_Head.Node.transform.localPosition, new Vector2(50, 50))))
		{
			Debug.LogError("Not found");
			targetNode = quadtree.GetRootNode();
		}

		float time2 = Time.realtimeSinceStartup;
		RaycastHit2D[] hits = Physics2D.CircleCastAll(m_Head.Node.transform.localPosition, 25.0f, Vector2.zero, Mathf.Infinity, 1 << (int)slConstants.Layer.Food);
		List<slFood> foods2 = new List<slFood>();
		for (int iHit = 0; iHit < hits.Length; iHit++)
		{
			slFood food = hits[iHit].collider.gameObject.GetComponent<slFood>();
			if ((food.transform.localPosition - m_Head.Node.transform.localPosition).sqrMagnitude <= 25.0f * 25.0f)
			{
				//food.BeEat(m_Head.Node.transform);
				foods2.Add(food);
			}
		}
		time2 = Time.realtimeSinceStartup - time2;

		float time1 = Time.realtimeSinceStartup;
		List<slFood> foods1 = new List<slFood>();
		foreach (slFood food in targetNode)
		{
			if ((food.transform.localPosition - m_Head.Node.transform.localPosition).sqrMagnitude <= 25.0f * 25.0f)
			{
				//	//food.BeEat(m_Head.Node.transform);
				foods1.Add(food);
			}
		}
		time1 = Time.realtimeSinceStartup - time1;

		Debug.LogError(string.Format("{0} - {1} - {2} - {3} - {4}", foods1.Count, foods2.Count, time1 < time2, time1, time2));
	}

	protected override void HandleInitialize(object additionalData)
	{
		InitializeAdditionalData initializeData = additionalData as InitializeAdditionalData;
		hwmDebug.Assert(initializeData.NodeCount >= slConstants.SNAKE_INITIALIZEZ_NODE_MINCOUNT, "initializeData.NodeCount >= slConstants.SNAKE_INITIALIZEZ_NODE_MINCOUNT");

		m_TweakableProperties = Instantiate(hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.SnakeTweakableProperties
					, initializeData.TweakableProperties)) as slSnakeTweakableProperties;

		if (slWorld.GetInstance().NeedPresentation())
		{
			m_Presentation = (Instantiate(hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Actor
					, "SnakePresentation_" + MyProperties.SnakeName)) as GameObject)
				.GetComponent<slSnakePresentation>();
			m_Presentation.gameObject.transform.SetParent(transform);
		}

		m_Power = initializeData.NodeCount * m_TweakableProperties.PowerToNode;

		m_Head = CreateNode<HeadNode>("Head"
			, m_Presentation != null ? m_Presentation.MyProperties.Head : null
			, MyProperties.HeadColliderRadius
			, initializeData.HeadPosition
			, initializeData.HeadRotation);

		m_Clothes = CreateNode<ClothesNode>("Clothes"
			, m_Presentation != null ? m_Presentation.MyProperties.Clothes : null
			, MyProperties.ClothesColliderRadius
			, initializeData.HeadPosition + initializeData.HeadRotation * new Vector3(0, -slConstants.SNAKE_NODE_TO_NODE_DISTANCE, 0)
			, initializeData.HeadRotation);

		m_Bodys = new hwmDeque<BodyNode>(16);
		Vector3 bodyToClothesOffset = initializeData.HeadRotation * new Vector3(0, -slConstants.SNAKE_NODE_TO_NODE_DISTANCE, 0);
		Vector3 bodyToBodyOffset = initializeData.HeadRotation * new Vector3(0, -slConstants.SNAKE_NODE_TO_NODE_DISTANCE, 0);
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
	}

	protected override void HandleDispose(object additionalData)
	{
		DisposeAdditionalData disposeAdditionalData = additionalData as DisposeAdditionalData;
		if (disposeAdditionalData.MyDeadType != DeadType.FinishGame)
		{
			slWorld.GetInstance().GetFoodSystem().AddCreateEvent(slFood.FoodType.Large, m_Head.Node.transform.position, MyProperties.DeadFoodColor);
			slWorld.GetInstance().GetFoodSystem().AddCreateEvent(slFood.FoodType.Large, m_Clothes.Node.transform.position, MyProperties.DeadFoodColor);
			foreach (BodyNode node in m_Bodys)
			{
				slWorld.GetInstance().GetFoodSystem().AddCreateEvent(slFood.FoodType.Large, node.Node.transform.position, MyProperties.DeadFoodColor);
			}
		}

		m_Controller.UnControllerSnake();
		m_Controller = null;

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

	protected void OnDrawGizmos()
	{
		Gizmos.color = Color.black;
		Gizmos.DrawLine(m_Head.Node.transform.localPosition, m_Head.Node.transform.localPosition + (Vector3)TargetMoveDirection * slConstants.SNAKE_DETECT_DISTANCE);
	}

	private T CreateNode<T>(string name, GameObject presentation, float colliderRadius, Vector3 position, Quaternion rotation) where T : BodyNode, new()
	{
		T node = new T();
		bool isHead = node is HeadNode;

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
			headNode.Predict = new GameObject("Predict");
			headNode.Predict.transform.SetParent(transform);
			headNode.Predict.transform.localPosition = position;
			headNode.Predict.transform.localRotation = rotation;
			headNode.Predict.layer = (int)slConstants.Layer.SnakePredict;
			headNode.PredictCollider = headNode.Predict.AddComponent<BoxCollider2D>();
			headNode.PredictCollider.size = new Vector2(colliderRadius * slConstants.SNAKE_PREDICT_SIZE_X, slConstants.SNAKE_NODE_TO_NODE_DISTANCE * slConstants.SNAKE_PREDICT_SIZE_Y);
			headNode.PredictCollider.offset = new Vector2(0, headNode.PredictCollider.size.y * 0.5f + colliderRadius);
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
				m_Power += food.BeEat(m_Head.Node.transform);
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

	private bool CanEnterSpeedUp()
	{
		return m_Power > m_TweakableProperties.SpeedUpEnterRequiredNode * m_TweakableProperties.PowerToNode;
	}

	private bool CanKeepSpeedUp()
	{
		return m_Power > m_TweakableProperties.SpeedUpKeepRequiredNode * m_TweakableProperties.PowerToNode;
	}

	[System.Serializable]
	public class Properties
	{
		public string SnakeName;

		public float HeadColliderRadius;
		public float ClothesColliderRadius;
		public float BodyColliderRadius;

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