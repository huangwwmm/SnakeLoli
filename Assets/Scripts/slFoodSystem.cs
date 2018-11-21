using System.Collections;
using UnityEngine;

public class slFoodSystem
{
	private hwmQuadtree m_Quadtree;
	private Pool m_Pool;
	private int m_MaxFood;

	private Vector2 m_FoodMinPosition;
	private Vector2 m_FoodMaxPosition;

	public void Initialize(slLevel level)
	{
		m_FoodMaxPosition = level.MapSize * 0.5f - slConstants.FOOD_MAP_EDGE;
		m_FoodMinPosition = -m_FoodMaxPosition;

		m_MaxFood = (int)(level.MapSize.x * level.MapSize.y / level.FoodDensity);

		m_Quadtree = new hwmQuadtree();
		m_Quadtree.Initialize(slConstants.FOOD_QUADTREE_MAXDEPTH
			, slConstants.FOOD_QUADTREE_MAXELEMENT_PERNODE
			, slConstants.FOOD_QUADTREE_MAXELEMENT_PREPARENTNODE
			, slConstants.FOOD_QUADTREE_LOOSESCALE
			, new hwmBounds2D(Vector2.zero, level.MapSize));

		m_Pool = new Pool(m_MaxFood);
	}

	public void Dispose()
	{
		m_Quadtree.Dispose();
		m_Quadtree = null;

		m_Pool.Dispose();
		m_Pool = null;
	}

	public hwmQuadtree GetQuadtree()
	{
		return m_Quadtree;
	}

	public void CreateFood(slFood.FoodType foodType, Vector3 position, Color color)
	{
		slFood food = m_Pool.Pop();
		food.ChangeFoodType(foodType, color);
		food.transform.position = position;
		food.UpdateQuadtreeElement();
	}

	public IEnumerator EnterMap_Co()
	{
		for (int iFood = 0; iFood < m_MaxFood; iFood++)
		{
			CreateFood(slFood.FoodType.Normal
				, hwmRandom.NextVector2(m_FoodMinPosition, m_FoodMaxPosition)
				, hwmRandom.RandColorRGB());

			if (iFood % 1000 == 0)
			{
				yield return null;
			}
		}
	}

	public class Pool : hwmPool<slFood>
	{
		public Pool(int initialSize = 4) : base(initialSize)
		{
		}

		protected override slFood HandleCreateItem()
		{
			slFood food = (Object.Instantiate(hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Game, "Food")) as GameObject).GetComponent<slFood>();
			food.Initialize();
			return food;
		}

		protected override void HandleDisposeItem(slFood item)
		{
			item.Dispose();
		}

		protected override void HandlePopItem(ref slFood item)
		{
			item.ActiveFood();
		}

		protected override void HandlePushItem(ref slFood item)
		{
			item.DeactiveFood();
		}
	}
}