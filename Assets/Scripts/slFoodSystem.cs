using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class slFoodSystem
{
	private slFoodProperties m_NormalFoodProperties;
	private slFoodProperties m_LargeFoodProperties;

	private hwmQuadtree<slFood> m_Quadtree;
	private Pool m_Pool;
	private int m_MaxFood;
	private int m_FoodCount;

	private Vector2 m_FoodMinPosition;
	private Vector2 m_FoodMaxPosition;

	private GameObject m_FoodRoot;

	private Queue<CreateEvent> m_CreateEvents;

	public void Initialize(slLevel level)
	{
		m_FoodRoot = new GameObject("Foods");

		m_FoodMaxPosition = level.MapSize * 0.5f - new Vector2(slConstants.FOOD_MAP_EDGE, slConstants.FOOD_MAP_EDGE);
		m_FoodMinPosition = -m_FoodMaxPosition;

		m_MaxFood = level.FoodCount;

		m_NormalFoodProperties = hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Game, "NormalFoodProperties") as slFoodProperties;
		m_LargeFoodProperties = hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Game, "LargeFoodProperties") as slFoodProperties;

		m_Quadtree = new hwmQuadtree<slFood>();
		m_Quadtree.Initialize(CalculateQuadtreeDepth()
			, slConstants.FOOD_QUADTREE_MAXELEMENT_PERNODE
			, slConstants.FOOD_QUADTREE_MINELEMENT_PREPARENTNODE
			, slConstants.FOOD_QUADTREE_LOOSESCALE
			, new hwmBounds2D(Vector2.zero, level.MapSize));

		m_Pool = new Pool(m_MaxFood);

		m_CreateEvents = new Queue<CreateEvent>();

		m_FoodCount = 0;
	}

	public void Dispose()
	{
		m_CreateEvents.Clear();
		m_CreateEvents = null;

		m_Quadtree.Dispose();
		m_Quadtree = null;

		m_Pool.Dispose();
		m_Pool = null;

		Object.Destroy(m_FoodRoot);
	}

	public void DoUpdate()
	{
		int canCreateFoodCount = slConstants.FOOD_SYSTEM_MAXCREATE_PREFRAME;
		int createEventCount = Mathf.Min(m_CreateEvents.Count, canCreateFoodCount);
		while (createEventCount-- > 0)
		{
			CreateEvent createEvent = m_CreateEvents.Dequeue();
			CreateFood(createEvent.FoodType, createEvent.Position, createEvent.Color);
		}
		canCreateFoodCount -= createEventCount;

		int needCreateFood = Mathf.Min(m_MaxFood - m_FoodCount, canCreateFoodCount);
		while (needCreateFood-- > 0)
		{
			CreateFood(slFood.FoodType.Normal
				, hwmRandom.RandVector2(m_FoodMinPosition, m_FoodMaxPosition)
				, hwmRandom.RandColorRGB());
		}
	}

	public hwmQuadtree<slFood> GetQuadtree()
	{
		return m_Quadtree;
	}

	public void DestroyFood(slFood food)
	{
		food.DeactiveFood();
		m_Pool.Push(food);

		m_FoodCount--;
	}

	public IEnumerator EnterMap_Co()
	{
		for (int iFood = 0; iFood < m_MaxFood; iFood++)
		{
			CreateFood(slFood.FoodType.Normal
				, hwmRandom.RandVector2(m_FoodMinPosition, m_FoodMaxPosition)
				, hwmRandom.RandColorRGB());

			if (iFood % 1000 == 0)
			{
				yield return null;
			}
		}
	}

	public GameObject GetFoodRoot()
	{
		return m_FoodRoot;
	}

	public void AddCreateEvent(slFood.FoodType foodType, Vector3 position, Color color)
	{
		if (CanCreateFoodAt(position))
		{
			CreateEvent createEvent = new CreateEvent();
			createEvent.FoodType = foodType;
			createEvent.Position = position;
			createEvent.Color = color;
			m_CreateEvents.Enqueue(createEvent);
		}
	}

	public int GetFoodCount()
	{
		return m_FoodCount;
	}

	private void CreateFood(slFood.FoodType foodType, Vector3 position, Color color)
	{
		slFood food = m_Pool.Pop();
		switch (foodType)
		{
			case slFood.FoodType.Normal:
				food.ActiveFood(m_NormalFoodProperties, position, color);
				break;
			case slFood.FoodType.Large:
				food.ActiveFood(m_LargeFoodProperties, position, color);
				break;
		}

		m_FoodCount++;
	}

	private bool CanCreateFoodAt(Vector2 position)
	{
		return position.x >= m_FoodMinPosition.x
			&& position.y >= m_FoodMinPosition.y
			&& position.x <= m_FoodMaxPosition.x
			&& position.y <= m_FoodMaxPosition.y;
	}

	private int CalculateQuadtreeDepth()
	{
		int depth = 1;
		float width = m_FoodMaxPosition.x - m_FoodMinPosition.x;
		float height = m_FoodMaxPosition.y - m_FoodMinPosition.y;
		while (true)
		{
			if (width < slConstants.FOOD_QUADTREE_MAXDEPTH_BOUNDESIZE
				|| height < slConstants.FOOD_QUADTREE_MAXDEPTH_BOUNDESIZE)
			{
				break;
			}
			width *= 0.5f;
			height *= 0.5f;
			depth++;
		}
		return depth;
	}

	public class Pool : hwmPool<slFood>
	{
		public Pool(int initialSize = 4) : base(initialSize)
		{
		}

		protected override slFood HandleCreateItem()
		{
			slFood food = (Object.Instantiate(hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Game, "Food")) as GameObject).GetComponent<slFood>();
			food.transform.SetParent(slWorld.GetInstance().GetFoodSystem().GetFoodRoot().transform);
			food.Initialize();
			return food;
		}

		protected override void HandleDisposeItem(slFood item)
		{
			item.Dispose();
		}

		protected override void HandlePopItem(ref slFood item)
		{
		}

		protected override void HandlePushItem(ref slFood item)
		{
		}
	}

	public struct CreateEvent
	{
		public Vector3 Position;
		public Color Color;
		public slFood.FoodType FoodType;
	}
}