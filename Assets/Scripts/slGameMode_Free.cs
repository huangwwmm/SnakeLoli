using System.Collections;
using UnityEngine;

public class slGameMode_Free : hwmGameMode 
{
	protected new slLevel_Free m_Level;
	protected new slGameState_Free m_GameState;
	private slMap m_Map;
	private slFoodSystem m_FoodSystem;

	protected override IEnumerator HandleInitGame()
	{
		m_Level = hwmSystem.GetInstance().GetWorld().GetLevel() as slLevel_Free;
		m_GameState = hwmSystem.GetInstance().GetWorld().GetGameState() as slGameState_Free;
		yield return null;

		m_Map = (Instantiate(hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Game, "Map")) as GameObject).GetComponent<slMap>();
		m_Map.Initialize(m_Level.MapSize, true);
		yield return null;

		m_FoodSystem = (Instantiate(hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Game, "FoodSystem")) as GameObject).GetComponent<slFoodSystem>();
	}
}