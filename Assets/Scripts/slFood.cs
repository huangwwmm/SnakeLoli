using UnityEngine;

public class slFood : hwmActor
{
	public CircleCollider2D NormalCollider;
	public CircleCollider2D LargeCollider;

	private slFoodPresentation m_Presentation;
	private hwmQuadtree.Element m_QuadtreeElement;

	public void Initialize()
	{
		if (slWorld.GetInstance().NeedPresentation())
		{
			m_Presentation = (Instantiate(hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Game, "FoodPresentation")) as GameObject).GetComponent<slFoodPresentation>();
			m_Presentation.transform.SetParent(transform);
		}

		m_QuadtreeElement = new hwmQuadtree.Element(slWorld.GetInstance().GetFoodSystem().GetQuadtree());
	}

	public void Dispose()
	{
		m_Presentation = null;

		m_QuadtreeElement.RemoveElement();
		m_QuadtreeElement = null;
	}

	public void ActiveFood()
	{
		gameObject.SetActive(true);
	}

	public void DeactiveFood()
	{
		gameObject.SetActive(false);
		gameObject.transform.position = slConstants.FOOD_DEACTIVE_POSITION;
	}

	public void ChangeFoodType(FoodType foodType, Color color)
	{
		switch (foodType)
		{
			case FoodType.Normal:
				NormalCollider.enabled = true;
				LargeCollider.enabled = false;
				m_QuadtreeElement.Bounds.extents = new Vector2(NormalCollider.radius, NormalCollider.radius);
				break;
			case FoodType.Large:
				NormalCollider.enabled = true;
				LargeCollider.enabled = false;
				m_QuadtreeElement.Bounds.extents = new Vector2(LargeCollider.radius, LargeCollider.radius);
				break;
		}

		if (m_Presentation)
		{
			m_Presentation.ChangeFoodType(foodType, color);
		}
	}

	public void UpdateQuadtreeElement()
	{
		m_QuadtreeElement.Bounds.center = transform.position;
		m_QuadtreeElement.UpdateElement();
	}

	public enum FoodType
	{
		Normal,
		Large,
	}
}