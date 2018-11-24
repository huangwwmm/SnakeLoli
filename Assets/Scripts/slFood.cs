using UnityEngine;

public class slFood : MonoBehaviour
{
	public CircleCollider2D Collider;

	private slFoodPresentation m_Presentation;
	private hwmQuadtree.Element m_QuadtreeElement;
	private FoodProperties m_Properties;
	private State m_State;
	private Transform m_BeEatTransform;

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
		m_QuadtreeElement = new hwmQuadtree.Element(slWorld.GetInstance().GetFoodSystem().GetQuadtree());
	}

	public void DeactiveFood()
	{
		m_State = State.Notset;

		if (m_QuadtreeElement != null)
		{
			m_QuadtreeElement.RemoveElement();
			m_QuadtreeElement = null;
		}

		gameObject.SetActive(false);
		gameObject.transform.position = slConstants.FOOD_DEACTIVE_POSITION;

		m_Properties = null;
	}

	public void ChangeFoodType(FoodProperties foodProperties, Color color)
	{
		m_Properties = foodProperties;
		Collider.radius = m_Properties.BeEatRadius;
		m_QuadtreeElement.Bounds.extents = new Vector2(m_Properties.SpriteRadius, m_Properties.SpriteRadius);

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
			m_BeEatTransform = beEatTransform;
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
				if ((moveToPosition - m_BeEatTransform.localPosition).sqrMagnitude < 0.1f)
				{
					slWorld.GetInstance().GetFoodSystem().DestroyFood(this);
				}
			}
			else
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
	}
}