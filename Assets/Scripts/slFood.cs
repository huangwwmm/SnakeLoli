using UnityEngine;

public class slFood : MonoBehaviour
{
	public CircleCollider2D Collider;

	private slFoodPresentation m_Presentation;
	private hwmQuadtree.Element m_QuadtreeElement;
	private FoodProperties m_Properties;
	private State m_State;
	private Transform m_BeEatTransform;
	private float m_BeEatAnimationRemainTime;
	private float m_RemainLifeTime;

	public void Initialize()
	{
		if (slWorld.GetInstance().NeedPresentation())
		{
			m_Presentation = (Instantiate(hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Game, "FoodPresentation")) as GameObject).GetComponent<slFoodPresentation>();
			m_Presentation.transform.SetParent(transform);
		}
	}

	public void Dispose()
	{
		m_Presentation = null;
	}

	public void ActiveFood()
	{
		m_State = State.Idle;

		gameObject.SetActive(true);
		Collider.enabled = true;

		m_QuadtreeElement = new hwmQuadtree.Element(slWorld.GetInstance().GetFoodSystem().GetQuadtree());
	}

	public void DeactiveFood()
	{
		if (m_QuadtreeElement != null)
		{
			m_QuadtreeElement.RemoveElement();
			m_QuadtreeElement = null;
		}

		Collider.enabled = false;
		gameObject.SetActive(false);
		gameObject.transform.position = slConstants.FOOD_DEACTIVE_POSITION;

		m_Properties = null;

		m_State = State.Notset;
	}

	public void ChangeFoodType(FoodProperties foodProperties, Color color)
	{
		m_Properties = foodProperties;
		Collider.radius = m_Properties.BeEatRadius;
		m_QuadtreeElement.Bounds.extents = new Vector2(m_Properties.SpriteRadius, m_Properties.SpriteRadius);
		m_RemainLifeTime = m_Properties.LifeTime;

		if (m_Presentation)
		{
			m_Presentation.ChangeFoodType(foodProperties, color);
		}
	}

	public void UpdateQuadtreeElement()
	{
		m_QuadtreeElement.Bounds.center = transform.position;
		m_QuadtreeElement.UpdateElement();
	}

	public int BeEat(Transform beEatTransform)
	{
		if (m_State == State.Idle)
		{
			m_State = State.BeEat;
			Collider.enabled = false;
			m_BeEatTransform = beEatTransform;
			m_BeEatAnimationRemainTime = Vector3.Distance(m_BeEatTransform.localPosition, transform.localPosition) / slConstants.FOOD_BEEAT_MOVE_SPEED;
			return m_Properties.AddPower;
		}
		else
		{
			return 0;
		}
	}

	protected void Update()
	{
		if (m_State == State.BeEat)
		{
			if (m_BeEatTransform != null)
			{
				Vector3 moveToPosition = Vector3.MoveTowards(transform.localPosition, m_BeEatTransform.localPosition, slConstants.FOOD_BEEAT_MOVE_SPEED * Time.deltaTime);
				transform.localPosition = moveToPosition;
				m_BeEatAnimationRemainTime -= Time.deltaTime;
				if (m_BeEatAnimationRemainTime <= 0
					|| (moveToPosition - m_BeEatTransform.localPosition).sqrMagnitude < 0.1f)
				{
					slWorld.GetInstance().GetFoodSystem().DestroyFood(this);
				}
			}
			else
			{
				slWorld.GetInstance().GetFoodSystem().DestroyFood(this);
			}
		}
		else if (m_State == State.Idle)
		{
			m_RemainLifeTime -= Time.deltaTime;
			if (m_RemainLifeTime < 0)
			{
				slWorld.GetInstance().GetFoodSystem().DestroyFood(this);
			}
		}
	}

	public enum FoodType
	{
		Normal,
		Large,
	}

	public enum State
	{
		Notset,
		Idle,
		BeEat
	}

	[System.Serializable]
	public class FoodProperties
	{
		public FoodType MyType;
		public float SpriteRadius;
		public float BeEatRadius;
		public int AddPower;
		public float LifeTime;
	}
}