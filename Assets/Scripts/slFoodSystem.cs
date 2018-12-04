using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class slFoodSystem
{

	private hwmQuadtree<slFood> m_Quadtree;
	private slFoodProperties[] m_FoodPropertiess;
	private FoodPool m_FoodPool;
	private FoodPresentationPool[] m_FoodPresentationPools;
	private int m_MaxFood;
	private int m_FoodCount;

	private Vector2 m_FoodMinPosition;
	private Vector2 m_FoodMaxPosition;

	private Transform m_FoodRoot;

	private hwmBetterDictionary<int, slFood> m_Foods;
	private Queue<CreateEvent> m_CreateEvents;
	private hwmBetterList<slFood> m_DestroyEvents;
	private int m_LastFoodIndex = 0;

	public void Initialize(slLevel level)
	{
		m_FoodRoot = new GameObject("Foods").transform;

		m_FoodMaxPosition = level.MapSize * 0.5f - new Vector2(slConstants.FOOD_MAP_EDGE, slConstants.FOOD_MAP_EDGE);
		m_FoodMinPosition = -m_FoodMaxPosition;

		m_MaxFood = level.FoodCount;

		m_FoodPropertiess = new slFoodProperties[(int)slConstants.FoodType.Count];
		for (int iFood = 0; iFood < m_FoodPropertiess.Length; iFood++)
		{
			m_FoodPropertiess[iFood] = hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Game
				, slConstants.FOOD_PROPERTIES_PREFAB_STARTWITHS + slConstants.FoodTypeToString((slConstants.FoodType)iFood)) as slFoodProperties;
		}

		m_Quadtree = new hwmQuadtree<slFood>();
		m_Quadtree.Initialize(CalculateQuadtreeDepth()
			, slConstants.FOOD_QUADTREE_MAXELEMENT_PERNODE
			, slConstants.FOOD_QUADTREE_MINELEMENT_PREPARENTNODE
			, new Vector2(slConstants.FOOD_QUADTREE_LOOSESIZE, slConstants.FOOD_QUADTREE_LOOSESIZE)
			, slWorld.GetInstance().GetMap().GetMapBox());
#if UNITY_EDITOR
		slQuadtreeGizmos.FoodQuadtree = m_Quadtree;
#endif

		m_FoodPool = new FoodPool(m_FoodRoot);
		m_FoodPool.Initialize(Mathf.CeilToInt(m_MaxFood * slConstants.FOOD_POOL_INITIALIZE_MULTIPLY));
		if (slWorld.GetInstance().NeedPresentation())
		{
			m_FoodPresentationPools = new FoodPresentationPool[(int)slConstants.FoodType.Count];
			for (int iFood = 0; iFood < m_FoodPropertiess.Length; iFood++)
			{
				slConstants.FoodType foodType = (slConstants.FoodType)iFood;
				m_FoodPresentationPools[iFood] = new FoodPresentationPool(m_FoodRoot, foodType);
				m_FoodPresentationPools[iFood].Initialize(0);
			}
		}

		m_CreateEvents = new Queue<CreateEvent>();
		m_Foods = new hwmBetterDictionary<int, slFood>();
		m_DestroyEvents = new hwmBetterList<slFood>();
		m_FoodCount = 0;
	}

	public void Dispose()
	{
		m_Foods.Clear();
		m_Foods = null;
		m_CreateEvents.Clear();
		m_CreateEvents = null;
		m_CreateEvents.Clear();
		m_CreateEvents = null;

		m_Quadtree.Dispose();
		m_Quadtree = null;

		m_FoodPool.Dispose();
		m_FoodPool = null;

		if (m_FoodPresentationPools != null)
		{
			for (int iFood = 0; iFood < m_FoodPresentationPools.Length; iFood++)
			{
				m_FoodPresentationPools[iFood].Dispose();
			}
			m_FoodPresentationPools = null;
		}

		Object.Destroy(m_FoodRoot);
	}

	public void DoUpdateFoods(float deltaTime)
	{
		hwmBetterDictionary<int, slFood>.Entry[] entries = m_Foods.GetEntries();
		for (int iFood = 0; iFood < entries.Length; iFood++)
		{
			if (entries[iFood].hashCode >= 0)
			{
				entries[iFood].value.DoUpdate(deltaTime);
			}
		}
		//foreach (slFood iterFood in m_Foods.Values)
		//{
		//	iterFood.DoUpdate(deltaTime);
		//}
	}

	public void DoUpdateFoodSystem()
	{
		int canCreateFoodCount = slConstants.FOOD_SYSTEM_MAXCREATE_PREFRAME;
		int createEventCount = Mathf.Min(m_CreateEvents.Count, canCreateFoodCount);
		while (createEventCount-- > 0)
		{
			CreateEvent createEvent = m_CreateEvents.Dequeue();
			CreateFood(createEvent.FoodType, createEvent.Position, createEvent.Color, createEvent.Power);
		}
		canCreateFoodCount -= createEventCount;

		int needCreateFood = Mathf.Min(m_MaxFood - m_FoodCount, canCreateFoodCount);
		while (needCreateFood-- > 0)
		{
			CreateFood(slConstants.FoodType.Normal
				, hwmRandom.RandVector2(m_FoodMinPosition, m_FoodMaxPosition)
				, hwmRandom.RandColorRGB()
				, slConstants.NORMAL_FOOD_POWER);
		}

		for (int iFood = 0; iFood < m_DestroyEvents.Count; iFood++)
		{
			slFood food = m_DestroyEvents[iFood];
			DestroyFood(food);
		}
		m_DestroyEvents.Clear();

		m_Quadtree.MergeAndSplitAllNode();
	}


	public hwmQuadtree<slFood> GetQuadtree()
	{
		return m_Quadtree;
	}

	public void AddDestroyFoodEvent(slFood food)
	{
		m_DestroyEvents.Add(food);
	}

	public IEnumerator EnterMap_Co()
	{
		System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
		stopwatch.Start();
		for (int iFood = 0; iFood < m_MaxFood; iFood++)
		{
			if (iFood > 0 && iFood % slConstants.FOOD_CREATECOUNT_PREFRAME_WHEN_ENDTERMAP == 0)
			{
				Debug.Log("foodsystem entermap created food count " + m_FoodCount);
				stopwatch.Stop();
				yield return null;
				stopwatch.Start();
			}

			CreateFood(slConstants.FoodType.Normal
				, hwmRandom.RandVector2(m_FoodMinPosition, m_FoodMaxPosition)
				, hwmRandom.RandColorRGB()
				, slConstants.NORMAL_FOOD_POWER);
		}

		m_Quadtree.AutoMergeAndSplitNode = false;
		stopwatch.Stop();
		Debug.Log(string.Format("Food system enter map used {0} ms", stopwatch.ElapsedMilliseconds));
	}

	public void AddCreateEvent(slConstants.FoodType foodType, Vector3 position, Color color, float power)
	{
		if (CanCreateFoodAt(position))
		{
			CreateEvent createEvent = new CreateEvent();
			createEvent.FoodType = foodType;
			createEvent.Position = position;
			createEvent.Color = color;
			createEvent.Power = power;
			m_CreateEvents.Enqueue(createEvent);
		}
	}

	public int GetFoodCount()
	{
		return m_FoodCount;
	}

	private void CreateFood(slConstants.FoodType foodType, Vector3 position, Color color, float power)
	{
		slFoodProperties foodProperties = m_FoodPropertiess[(int)foodType];
		slFood food = m_FoodPool.Pop();
		int foodIndex = food.GetIndex();
		if (foodIndex == slConstants.FOOD_NOTSET_INDEX)
		{
			foodIndex = ++m_LastFoodIndex;
		}
		m_Foods.Add(foodIndex, food);
		slFoodPresentation foodPresentation = m_FoodPresentationPools != null ? m_FoodPresentationPools[(int)foodType].Pop() : null;
		if (foodPresentation != null)
		{
			foodPresentation.gameObject.SetActive(true);

			foodPresentation.SetColor(color);
		}
		food.ActiveFood(foodIndex, foodProperties, foodPresentation, position, color, power);
		m_FoodCount++;
	}

	private void DestroyFood(slFood food)
	{
		slFoodPresentation foodPresentation = food.GetPresentation();
		if (foodPresentation != null)
		{
			foodPresentation.gameObject.SetActive(false);
			m_FoodPresentationPools[(int)foodPresentation.FoodType].Push(foodPresentation);
		}
		m_Foods.Remove(food.GetIndex());
		food.DeactiveFood();
		m_FoodPool.Push(food);

		m_FoodCount--;
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
			if (width < slConstants.FOOD_QUADTREE_MAXDEPTH_BOXSIZE
				|| height < slConstants.FOOD_QUADTREE_MAXDEPTH_BOXSIZE)
			{
				break;
			}
			width *= 0.5f;
			height *= 0.5f;
			depth++;
		}
		return depth;
	}

	public class FoodPool : hwmPool<slFood>
	{
		private Transform m_Root;

		public FoodPool(Transform root)
		{
			m_Root = root;
		}

		protected override slFood HandleCreateItem()
		{
			GameObject go = new GameObject("food");
			go.transform.SetParent(m_Root, false);
			go.SetActive(false);

			slFood food = go.AddComponent<slFood>();
			return food;
		}
	}

	public class FoodPresentationPool : hwmPool<slFoodPresentation>
	{
		private slConstants.FoodType m_FoodType;
		private Transform m_Root;

		public FoodPresentationPool(Transform root, slConstants.FoodType foodType)
		{
			m_Root = root;
			m_FoodType = foodType;
		}

		protected override slFoodPresentation HandleCreateItem()
		{
			slFoodPresentation food = (Object.Instantiate(hwmSystem.GetInstance().GetAssetLoader()
					.LoadAsset(hwmAssetLoader.AssetType.Game, slConstants.FOOD_PRESENTATION_PREFAB_STARTWITHS + slConstants.FoodTypeToString(m_FoodType))) as GameObject)
				.GetComponent<slFoodPresentation>();
			food.transform.SetParent(m_Root, false);
			food.gameObject.SetActive(false);
			return food;
		}
	}

	public struct CreateEvent
	{
		public Vector3 Position;
		public Color Color;
		public slConstants.FoodType FoodType;
		public float Power;
	}
}