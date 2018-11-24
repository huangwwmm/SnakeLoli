using System.Collections;
using UnityEngine;

public class slWorld : hwmWorld
{
	protected new static slWorld ms_Instance;

	protected new slLevel m_Level;
	private slPlayer m_Player;
	private slMap m_Map;
	private slFoodSystem m_FoodSystem;

	public new static slWorld GetInstance()
	{
		return ms_Instance;
	}

	public slWorld() : base()
	{
		ms_Instance = this;
	}

	public new slLevel GetLevel()
	{
		return m_Level;
	}

	public slMap GetMap()
	{
		return m_Map;
	}

	public slPlayer GetPlayer()
	{
		return m_Player;
	}

	public slFoodSystem GetFoodSystem()
	{
		return m_FoodSystem;
	}

	protected override IEnumerator HandleBeginPlay_Co()
	{
		m_Level = base.m_Level as slLevel;

		m_Player = (Object.Instantiate(hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Game, "Player")) as GameObject).GetComponent<slPlayer>();
		m_Player.Initialize();

		m_Map = (Object.Instantiate(hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Game, "Map")) as GameObject).GetComponent<slMap>();
		m_Map.Initialize(m_Level.MapSize);
		yield return null;

		m_FoodSystem = new slFoodSystem();
		m_FoodSystem.Initialize(m_Level);
	}
}