using System;
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

	private slSnakeSystem.NodePool<BodyNode> m_BodyPool;

	private AliveState m_AliveState;

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
		return m_Head.GetPosition();
	}

	public float GetHeadRadius()
	{
		return m_Head.Radius;
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
			case SpeedState.Still:
				return;
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
			Vector3 lastNodePosition = m_Head.GetPosition();
			Quaternion lastNodeRotation = m_Head.GetRotation();
			Quaternion headRotation = Quaternion.Euler(0, 0, -Vector2.SignedAngle(m_CurrentMoveDirection, Vector2.up));
			m_Head.SetPositionAndRotation(lastNodePosition + headRotation * new Vector2(0, slConstants.SNAKE_NODE_TO_NODE_DISTANCE)
				, headRotation);

			Vector3 swapNodePosition = m_Clothes.GetPosition();
			Quaternion swapNodeRotation = m_Clothes.GetRotation();
			m_Clothes.SetPositionAndRotation(lastNodePosition, lastNodeRotation);
			lastNodePosition = swapNodePosition;
			lastNodeRotation = swapNodeRotation;

			if (m_Bodys.Count > 0)
			{
				BodyNode newFrontNode;
				if (bodyCountOffset > 0)
				{
					bodyCountOffset--;
					newFrontNode = m_BodyPool.Pop();
					newFrontNode.Active(this);
				}
				else
				{
					newFrontNode = m_Bodys.PopBack();
				}
				newFrontNode.SetPositionAndRotation(lastNodePosition, lastNodeRotation);

				BodyNode oldFrontNode = m_Bodys.PeekFront();
				m_Bodys.PushFront(newFrontNode);
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
			backNode.Deactive();
			m_BodyPool.Push(backNode);
		}
	}

	public void DoUpdateEatFood()
	{
		EatFood(m_TweakableProperties.EatFoodRadius);
	}

	public void DoUpdateCollide()
	{
		hwmBox2D collideAABB = hwmBox2D.BuildAABB(m_Head.GetPosition(), new Vector2(m_Head.Radius, m_Head.Radius));
		hwmSphere2D headSphere = new hwmSphere2D(m_Head.GetPosition(), m_Properties.HeadColliderRadius);
		if (!slWorld.GetInstance().GetMap().GetMapBox().IsInsideOrOn(m_Head.AABB))
		{
			m_AliveState = AliveState.DeadHitWall;
			return;
		}
		hwmQuadtree<QuadtreeElement>.AABBEnumerator enumerator = new hwmQuadtree<QuadtreeElement>.AABBEnumerator(m_Head.OwnerQuadtree.GetRootNode()
			, collideAABB);
		while (enumerator.MoveNext())
		{
			hwmQuadtree<QuadtreeElement>.Node iterNode = enumerator.Current;
			hwmBetterList<QuadtreeElement> elements = iterNode.GetElements();
			for (int iElement = elements.Count - 1; iElement >= 0; iElement--)
			{
				QuadtreeElement iterElement = elements[iElement];
				if (iterElement.Owner != m_Guid
					&& iterElement.NodeType != slConstants.NodeType.Predict
					&& (iterElement.GetPosition() - m_Head.GetPosition()).sqrMagnitude
						<= ((m_Head.Radius + iterElement.Radius) * (m_Head.Radius + iterElement.Radius)))
				{
					m_AliveState = AliveState.DeadHitSnake;
				}
			}
		}
	}

	public slBaseController GetController()
	{
		return m_Controller;
	}

	public void EatFood(float radius)
	{
		hwmBox2D eatAABB = hwmBox2D.BuildAABB(m_Head.GetPosition(), new Vector2(radius, radius));
		hwmQuadtree<slFood>.AABBEnumerator enumerator = new hwmQuadtree<slFood>.AABBEnumerator(slWorld.GetInstance().GetFoodSystem().GetQuadtree().GetRootNode()
			, eatAABB);
		hwmSphere2D headSphere = new hwmSphere2D(m_Head.GetPosition(), radius);
		while (enumerator.MoveNext())
		{
			hwmQuadtree<slFood>.Node iterNode = enumerator.Current;
			hwmBetterList<slFood> foods = iterNode.GetElements();
			bool inHeadSphere = headSphere.IsInside(iterNode.GetLooseBox());
			for (int iFood = foods.Count - 1; iFood >= 0; iFood--)
			{
				slFood iterFood = foods[iFood];
				if (inHeadSphere
					|| (iterFood.GetPosition() - m_Head.GetPosition()).sqrMagnitude <= radius * radius)
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

	public bool IsAlive()
	{
		return m_AliveState == AliveState.Alive;
	}

	public void Dead()
	{
		hwmWorld.GetInstance().DestroyActor(this, null);
	}

	public void VaildNode()
	{
		hwmDebug.Assert(m_Head.Owner == m_Guid, "VaildNode");
		hwmDebug.Assert(m_Head.NodeType == slConstants.NodeType.Head , "VaildNode");
		hwmDebug.Assert(m_Head.OwnerQuadtreeNode != null, "VaildNode");
		hwmDebug.Assert(m_Head.PredictNode.Owner == m_Guid, "VaildNode");
		hwmDebug.Assert(m_Head.PredictNode.NodeType ==  slConstants.NodeType.Predict, "VaildNode");

		hwmDebug.Assert(m_Clothes.Owner == m_Guid, "VaildNode");
		hwmDebug.Assert(m_Clothes.NodeType == slConstants.NodeType.Clothes, "VaildNode");
		hwmDebug.Assert(m_Clothes.OwnerQuadtreeNode != null, "VaildNode");

		foreach (BodyNode body in m_Bodys)
		{
			hwmDebug.Assert(body.Owner == m_Guid, "VaildNode");
			hwmDebug.Assert(body.NodeType == slConstants.NodeType.Body, "VaildNode");
			hwmDebug.Assert(body.OwnerQuadtreeNode != null, "VaildNode");
		}
	}

	protected override void HandleInitialize(object additionalData)
	{
		InitializeAdditionalData initializeData = additionalData as InitializeAdditionalData;
		hwmDebug.Assert(initializeData.NodeCount >= slConstants.SNAKE_INITIALIZEZ_NODE_MINCOUNT, "initializeData.NodeCount >= slConstants.SNAKE_INITIALIZEZ_NODE_MINCOUNT");

		m_AliveState = AliveState.Alive;

		m_SnakeName = initializeData.SnakeName;
		m_TweakableProperties = slWorld.GetInstance().GetSnakeSystem().GetTweakableProperties(initializeData.TweakableProperties);
		m_Properties = slWorld.GetInstance().GetSnakeSystem().GetProperties(m_SnakeName);

		m_Power = initializeData.NodeCount * m_TweakableProperties.NodeToPower;

		#region Head
		m_Head = slWorld.GetInstance().GetSnakeSystem().GetHeadPool(m_SnakeName).Pop();
		m_Head.Active(this);
		m_Head.SetPositionAndRotation(initializeData.HeadPosition, initializeData.HeadRotation);
		#endregion

		#region Clothes
		m_Clothes = slWorld.GetInstance().GetSnakeSystem().GetClothesPool(m_SnakeName).Pop();
		m_Clothes.Active(this);
		m_Clothes.SetPositionAndRotation(initializeData.HeadPosition + initializeData.HeadRotation * new Vector3(0, -slConstants.SNAKE_NODE_TO_NODE_DISTANCE, 0)
			, initializeData.HeadRotation);
		#endregion

		#region Body
		m_BodyPool = slWorld.GetInstance().GetSnakeSystem().GetBodyPool(m_SnakeName);

		m_Bodys = new hwmDeque<BodyNode>(16);
		Vector3 bodyToClothesOffset = initializeData.HeadRotation * new Vector3(0, -slConstants.SNAKE_NODE_TO_NODE_DISTANCE, 0);
		Vector3 bodyToBodyOffset = initializeData.HeadRotation * new Vector3(0, -slConstants.SNAKE_NODE_TO_NODE_DISTANCE, 0);
		for (int iNode = 3; iNode <= initializeData.NodeCount; iNode++) // MagicNumber: 1->Head 2->Clothes 3->FirstBody
		{
			BodyNode node = m_BodyPool.Pop();
			node.Active(this);
			node.SetPositionAndRotation(m_Clothes.GetPosition() + bodyToClothesOffset + bodyToBodyOffset * (iNode - 3)
				, initializeData.HeadRotation);
			if (node.Sprite != null)
			{
				node.Sprite.sortingOrder = slConstants.SNAKE_SPRITERENDERER_MIN_ORDERINLAYER + initializeData.NodeCount - iNode;
			}

			m_Bodys.PushBack(node);
		}

		#endregion

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
		slConstants.FoodType foodType = m_IsReaminsFoodContamination ? slConstants.FoodType.Contamination : slConstants.FoodType.Remains;
		float power = m_IsReaminsFoodContamination ? m_RemainsFoodContaminationPower : m_TweakableProperties.RemainsFoodPower;
		if (m_AliveState != AliveState.DeadFinishGame)
		{
			slWorld.GetInstance().GetFoodSystem().AddCreateEvent(foodType, m_Head.GetPosition(), m_Properties.DeadFoodColor, power);
			slWorld.GetInstance().GetFoodSystem().AddCreateEvent(foodType, m_Clothes.GetPosition(), m_Properties.DeadFoodColor, power);
			foreach (BodyNode node in m_Bodys)
			{
				slWorld.GetInstance().GetFoodSystem().AddCreateEvent(foodType, node.GetPosition(), m_Properties.DeadFoodColor, power);
			}
		}

		m_SkillEventArgs = null;

		m_Controller.UnControllerSnake();
		m_Controller = null;

		m_Head.Deactive();
		slWorld.GetInstance().GetSnakeSystem().GetHeadPool(m_SnakeName).Push(m_Head);
		m_Head = null;

		m_Clothes.Deactive();
		slWorld.GetInstance().GetSnakeSystem().GetClothesPool(m_SnakeName).Push(m_Clothes);
		m_Clothes = null;

		int bodyCount = m_Bodys.Count;
		for (int iBody = 0; iBody < bodyCount; iBody++)
		{
			BodyNode bodyNode = m_Bodys.PopBack();
			bodyNode.Deactive();
			m_BodyPool.Push(bodyNode);
		}
		m_Bodys.Clear();
		m_Bodys = null;

		Destroy(m_TweakableProperties);
		m_TweakableProperties = null;

		Destroy(m_TweakableProperties);
		m_Properties = null;
	}

#if UNITY_EDITOR
	protected void OnDrawGizmos()
	{
		Gizmos.color = Color.black;
		hwmUtility.GizmosDrawRotateBox2D(m_Head.PredictNode.Box, m_Head.PredictNode.GetRotation());
	}
#endif

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

	public class HeadNode : BodyNode
	{
		public new SpriteRenderer[] Sprite;
		public PredictNode PredictNode;

		public override void SetPositionAndRotation(Vector3 position, Quaternion rotation)
		{
			PredictNode.SetPositionAndRotation(position, rotation);
			base.SetPositionAndRotation(position, rotation);
		}

		public override void Active(slSnake owner)
		{
			base.Active(owner);

			Radius = owner.GetProperties().HeadColliderRadius;

			PredictNode.Active(owner);
		}

		public override void Deactive()
		{
			PredictNode.Deactive();
			base.Deactive();
		}

		protected override void UpdateQuadtree()
		{
			base.UpdateQuadtree();
			PredictNode.UpdateQuadtree();
		}
	}

	public class ClothesNode : BodyNode
	{
		public override void Active(slSnake owner)
		{
			base.Active(owner);

			Radius = owner.GetProperties().ClothesColliderRadius;
		}

		public override void Deactive()
		{
			base.Deactive();
		}
	}

	public class BodyNode : QuadtreeElement
	{
		public GameObject Node;
		public SpriteRenderer Sprite;

		public override void SetPositionAndRotation(Vector3 position, Quaternion rotation)
		{
			base.SetPositionAndRotation(position, rotation);

			Node.transform.localPosition = position;
			Node.transform.localRotation = rotation;

			UpdateQuadtree();
		}

		public override void Active(slSnake owner)
		{
			base.Active(owner);
			Node.name = owner.GetGuidStr();
			Node.SetActive(true);

			Radius = owner.GetProperties().BodyColliderRadius;
		}

		public override void Deactive()
		{
			Node.SetActive(false);

			base.Deactive();
		}

		protected virtual void UpdateQuadtree()
		{
			AABB = hwmBox2D.BuildAABB(m_Position, new Vector2(Radius, Radius));
			OwnerQuadtree.UpdateElement(this);
		}
	}

	public class PredictNode : QuadtreeElement
	{
		public hwmBox2D Box;
		public Vector2 Extent;
		public Vector2 Offset;

		public override void SetPositionAndRotation(Vector3 position, Quaternion rotation)
		{
			m_Position = position + rotation * Offset;
			m_Rotation = rotation;
		}

		public void UpdateQuadtree()
		{
			Box = hwmBox2D.BuildAABB(m_Position, Extent);
			AABB = m_Rotation * Box;

			OwnerQuadtree.UpdateElement(this);
		}

		public override void Active(slSnake owner)
		{
			base.Active(owner);

			Extent = new Vector2(Radius * slConstants.SNAKE_PREDICT_SIZE_X
				, slConstants.SNAKE_NODE_TO_NODE_DISTANCE * slConstants.SNAKE_PREDICT_SIZE_Y) * 0.5f;
			Offset = new Vector2(0, Extent.y + Radius);
		}
	}

	public class QuadtreeElement : hwmQuadtree<QuadtreeElement>.IElement
	{
		public hwmQuadtree<QuadtreeElement> OwnerQuadtree { get; set; }
		public hwmQuadtree<QuadtreeElement>.Node OwnerQuadtreeNode { get; set; }
		public hwmBox2D AABB { get; set; }
		public int Owner;
		public slConstants.NodeType NodeType;

		public float Radius;

		protected Vector3 m_Position;
		protected Quaternion m_Rotation;

		public virtual void SetPositionAndRotation(Vector3 position, Quaternion rotation)
		{
			m_Position = position;
			m_Rotation = rotation;
		}

		public Vector3 GetPosition()
		{
			return m_Position;
		}

		public Quaternion GetRotation()
		{
			return m_Rotation;
		}

		public virtual void Active(slSnake owner)
		{
			Owner = owner.GetGuid();
			Radius = owner.GetProperties().BodyColliderRadius;
		}

		public virtual void Deactive()
		{
			OwnerQuadtree.RemoveElement(this);
			Owner = int.MinValue;
		}
	}


	public enum AliveState
	{
		Alive,
		DeadHitWall,
		DeadHitSnake,
		DeadFinishGame
	}

	public enum SpeedState
	{
		Still,
		Normal,
		SpeedUp,
	}
}