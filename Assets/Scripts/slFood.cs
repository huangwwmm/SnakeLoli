using UnityEngine;

public class slFood : MonoBehaviour, hwmQuadtree<slFood>.IElement
{
	public CircleCollider2D Collider;

	private slFoodPresentation m_Presentation;
	private slFoodProperties m_Properties;
	private State m_State;
	private Transform m_BeEatTransform;
	private float m_BeEatRemainTime;
	private float m_RemainLifeTime;

	public hwmQuadtree<slFood> OwnerQuadtree { get; set; }
	public hwmBounds2D QuadtreeNodeBounds { get; set; }
	public hwmQuadtree<slFood>.Node OwnerQuadtreeNode { get; set; }

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

	public void ActiveFood(slFoodProperties foodProperties, Vector3 position, Color color)
	{
		m_State = State.Idle;

		gameObject.SetActive(true);
		Collider.enabled = true;

		m_Properties = foodProperties;
		Collider.radius = m_Properties.BeEatRadius;
		m_RemainLifeTime = hwmRandom.RandFloat(m_Properties.MinLifeTime, m_Properties.MaxLifeTime);
		transform.localPosition = position;

		if (m_Presentation)
		{
			m_Presentation.ChangeFoodType(foodProperties, color);
		}

		OwnerQuadtree = slWorld.GetInstance().GetFoodSystem().GetQuadtree();
		QuadtreeNodeBounds = new hwmBounds2D(transform.localPosition, new Vector2(m_Properties.SpriteRadius, m_Properties.SpriteRadius) * 2.0f);
		OwnerQuadtree.UpdateElement(this);
	}

	public void DeactiveFood()
	{
		OwnerQuadtree.RemoveElement(this);
		OwnerQuadtree = null;

		Collider.enabled = false;
		gameObject.SetActive(false);
		gameObject.transform.position = slConstants.FOOD_DEACTIVE_POSITION;

		m_Properties = null;

		m_State = State.Notset;
	}

	public int BeEat(Transform beEatTransform)
	{
		if (m_State == State.Idle)
		{
			m_State = State.BeEat;
			Collider.enabled = false;
			m_BeEatTransform = beEatTransform;
			m_BeEatRemainTime = slConstants.FOOD_BEEAT_MOVE_TIME;
			return m_Properties.AddPower;
		}
		else
		{
			return 0;
		}
	}

	public slFoodPresentation GetPresentation()
	{
		return m_Presentation;
	}

	protected void Update()
	{
		if (m_State == State.BeEat)
		{
			if (m_BeEatTransform != null)
			{
				Vector3 meToTarget = m_BeEatTransform.localPosition - transform.localPosition;
				transform.localPosition = transform.localPosition + meToTarget * (Time.deltaTime / m_BeEatRemainTime);

				m_BeEatRemainTime -= Time.deltaTime;
				if (m_BeEatRemainTime <= 0)
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
}