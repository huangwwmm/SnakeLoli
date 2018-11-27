using System.Collections;
using UnityEngine;

public class slWorld : hwmWorld
{
	protected new static slWorld ms_Instance;

	private slMap m_Map;
	private slFoodSystem m_FoodSystem;
	private slPlayerController m_PlayerController;
	private slUpdateSchedule m_UpdateSchedule;

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
		return m_Level as slLevel;
	}

	public new slGameMode_Free GetGameMode()
	{
		return m_GameMode as slGameMode_Free;
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

	public slUpdateSchedule GetUpdateSchedule()
	{
		return m_UpdateSchedule;
	}

	protected override IEnumerator HandleAfterBeginPlay_Co()
	{
		m_GameMode = base.m_GameMode as slGameMode_Free;

		hwmSystem.GetInstance().GetInput().JoystickCursor.SetAvailable(false);
		hwmSystem.GetInstance().GetInput().SetAllAxisEnable(false);
		hwmSystem.GetInstance().GetInput().SetAllButtonEnable(false);

		m_PlayerController = (Object.Instantiate(hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Game, "PlayerController")) as GameObject)
			.GetComponent<slPlayerController>();
		m_PlayerController.Initialize();

		m_Level = base.m_Level as slLevel;

		m_Map = (Object.Instantiate(hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Game, "Map")) as GameObject).GetComponent<slMap>();
		m_Map.Initialize(GetLevel().MapSize);
		yield return null;

		m_FoodSystem = new slFoodSystem();
		m_FoodSystem.Initialize(GetLevel());

		m_UpdateSchedule = gameObject.AddComponent<slUpdateSchedule>();
	}

	protected override IEnumerator HandleBeforeEndPlay_Co()
	{
		Destroy(m_UpdateSchedule);
		m_UpdateSchedule = null;

		m_FoodSystem.Dispose();
		m_FoodSystem = null;

		m_Map.Dispose();
		Destroy(m_Map);
		m_Map = null;
		yield return null;

		m_PlayerController.Dispose();

		hwmSystem.GetInstance().GetInput().SetAllAxisEnable(false);
		hwmSystem.GetInstance().GetInput().JoystickCursor.SetAvailable(true);
	}
}