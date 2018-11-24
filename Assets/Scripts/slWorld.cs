﻿using System.Collections;
using UnityEngine;

public class slWorld : hwmWorld
{
	protected new static slWorld ms_Instance;

	protected new slLevel m_Level;
	private slMap m_Map;
	private slFoodSystem m_FoodSystem;
	private slPlayerController m_PlayerController;

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

	public slFoodSystem GetFoodSystem()
	{
		return m_FoodSystem;
	}

	public slPlayerController GetPlayerController()
	{
		return m_PlayerController;
	}

	protected override IEnumerator HandleBeginPlay_Co()
	{
		hwmSystem.GetInstance().GetInput().JoystickCursor.SetAvailable(false);
		hwmSystem.GetInstance().GetInput().SetAllAxisEnable(true);

		m_PlayerController = (Object.Instantiate(hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Game, "PlayerController")) as GameObject)
			.GetComponent<slPlayerController>();
		m_PlayerController.Initialize();

		m_Level = base.m_Level as slLevel;

		m_Map = (Object.Instantiate(hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Game, "Map")) as GameObject).GetComponent<slMap>();
		m_Map.Initialize(m_Level.MapSize);
		yield return null;

		m_FoodSystem = new slFoodSystem();
		m_FoodSystem.Initialize(m_Level);
	}

	protected override IEnumerator HandleEndPlay_Co()
	{
		m_FoodSystem.Dispose();
		m_Map.Dispose();
		yield return null;

		m_Level = null;
		m_PlayerController.Dispose();

		hwmSystem.GetInstance().GetInput().SetAllAxisEnable(false);
		hwmSystem.GetInstance().GetInput().JoystickCursor.SetAvailable(true);
	}
}