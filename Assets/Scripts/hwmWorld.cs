using System.Collections;
using UnityEngine;

public class hwmWorld
{
	private hwmLevel m_Level;
	private GameObject m_GameplayFrameworkObject;
	private hwmGameMode m_GameMode;
	private hwmGameState m_GameState;

	public IEnumerator BeginPlay()
	{
		yield return null;
		m_Level = hwmSystem.GetInstance().GetWaitingToPlayLevel();
		hwmDebug.Assert(m_Level != null, "m_Level != null");

		m_GameplayFrameworkObject = new GameObject();
		m_GameMode = m_GameplayFrameworkObject.AddComponent(System.Type.GetType(m_Level.GameModeClassName)) as hwmGameMode;
	}

	public IEnumerator EndPlay()
	{
		Object.Destroy(m_GameMode);

		Object.Destroy(m_GameplayFrameworkObject);

		m_Level = null;
		yield return null;
	}
}