using UnityEngine;

public class slFood : MonoBehaviour, hwmQuadtree<slFood>.IElement
{
	private slFoodPresentation m_Presentation;
	private slFoodProperties m_Properties;
	private State m_State;
	private Transform m_BeEatTransform;
	private float m_BeEatRemainTime;
	private float m_RemainLifeTime;
	private float m_Power = 1;
	private int m_Index;
	private Vector3 m_Position;

	public hwmQuadtree<slFood> OwnerQuadtree { get; set; }
	public hwmBox2D AABB { get; set; }
	public hwmQuadtree<slFood>.Node OwnerQuadtreeNode { get; set; }

	public void ActiveFood(int index, slFoodProperties foodProperties, slFoodPresentation foodPresentation, Vector3 position, Color color, float power)
	{
		m_Index = index;
		m_State = State.Idle;

		gameObject.SetActive(true);

		m_Properties = foodProperties;
		m_RemainLifeTime = hwmRandom.RandFloat(m_Properties.MinLifeTime, m_Properties.MaxLifeTime);

		m_Presentation = foodPresentation;
		SetPosition(position);

		OwnerQuadtree = slWorld.GetInstance().GetFoodSystem().GetQuadtree();
		AABB = hwmBox2D.BuildAABB(transform.localPosition, new Vector2(m_Properties.Radius, m_Properties.Radius));
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

		gameObject.SetActive(false);

		m_Properties = null;

		m_State = State.Notset;
	}

	public float BeEat(Transform beEatTransform)
	{
		if (m_State == State.Idle)
		{
			m_State = State.BeEat;
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

	public void DoUpdate(float deltaTime)
	{
		if (m_State == State.BeEat)
		{
			if (m_BeEatTransform != null)
			{
				Vector3 meToTarget = m_BeEatTransform.localPosition - transform.localPosition;
				SetPosition(transform.localPosition + meToTarget * (deltaTime / m_BeEatRemainTime));

				m_BeEatRemainTime -= deltaTime;
				if (m_BeEatRemainTime <= 0)
				{
					slWorld.GetInstance().GetFoodSystem().AddDestroyFoodEvent(this);
				}
			}
			else
			{
				slWorld.GetInstance().GetFoodSystem().AddDestroyFoodEvent(this);
			}
		}
		else if (m_State == State.Idle)
		{
			m_RemainLifeTime -= deltaTime;
			if (m_RemainLifeTime < 0)
			{
				slWorld.GetInstance().GetFoodSystem().AddDestroyFoodEvent(this);
			}
		}
	}

	public int GetIndex()
	{
		return m_Index;
	}

	public Vector3 GetPosition()
	{
		return m_Position;
	}

	private void SetPosition(Vector3 position)
	{
		m_Position = position;

		transform.localPosition = position;
		if (m_Presentation)
		{
			m_Presentation.transform.localPosition = position;
		}
	}

	public enum State
	{
		Notset,
		Idle,
		BeEat
	}
}