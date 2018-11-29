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
	private float m_Power = 1;

	public hwmQuadtree<slFood> OwnerQuadtree { get; set; }
	public hwmBounds2D QuadtreeNodeBounds { get; set; }
	public hwmQuadtree<slFood>.Node OwnerQuadtreeNode { get; set; }

	public void ActiveFood(slFoodProperties foodProperties, slFoodPresentation foodPresentation, Vector3 position, Color color, float power)
	{
		m_State = State.Idle;

		gameObject.SetActive(true);
		Collider.enabled = true;

		m_Properties = foodProperties;
		Collider.radius = m_Properties.BeEatRadius;
		m_RemainLifeTime = hwmRandom.RandFloat(m_Properties.MinLifeTime, m_Properties.MaxLifeTime);
		transform.localPosition = position;

		m_Presentation = foodPresentation;

		OwnerQuadtree = slWorld.GetInstance().GetFoodSystem().GetQuadtree();
		QuadtreeNodeBounds = new hwmBounds2D(transform.localPosition, new Vector2(m_Properties.SpriteRadius, m_Properties.SpriteRadius) * 2.0f);
		OwnerQuadtree.UpdateElement(this);

		m_Power = power;
	}

	public void DeactiveFood()
	{
		if (OwnerQuadtree != null)
		{
			OwnerQuadtree.RemoveElement(this);
			OwnerQuadtree = null;
		}

		Collider.enabled = false;
		gameObject.SetActive(false);
		gameObject.transform.localPosition = slConstants.FOOD_DEACTIVE_POSITION;

		m_Properties = null;

		m_State = State.Notset;
	}

	public float BeEat(Transform beEatTransform)
	{
		if (m_State == State.Idle)
		{
			m_State = State.BeEat;
			Collider.enabled = false;
			OwnerQuadtree.RemoveElement(this);
			OwnerQuadtree = null;
			m_BeEatTransform = beEatTransform;
			m_BeEatRemainTime = slConstants.FOOD_BEEAT_MOVE_TIME;
			return m_Power;
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
		Normal = 0,
		Remains,
		Contamination,
		/// <summary>
		/// must end
		/// </summary>
		Count,
	}

	public enum State
	{
		Notset,
		Idle,
		BeEat
	}
}