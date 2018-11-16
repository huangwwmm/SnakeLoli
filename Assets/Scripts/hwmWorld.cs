using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// TODO support multiplayer
/// </summary>
public class hwmWorld
{
	private hwmLevel m_Level;
	private GameObject m_GameplayFrameworkObject;
	private hwmGameState m_GameState;
	private hwmGameMode m_GameMode;

	public IEnumerator BeginPlay()
	{
		m_Level = hwmSystem.GetInstance().GetWaitingToPlayLevel();
		hwmDebug.Assert(m_Level != null, "m_Level != null");

		m_GameplayFrameworkObject = new GameObject("Gameplay Framework");

		m_GameState = Activator.CreateInstance(Type.GetType(m_Level.GameStateClassName)) as hwmGameState;
		m_GameState.Initialize();
		m_GameMode = m_GameplayFrameworkObject.AddComponent(Type.GetType(m_Level.GameModeClassName)) as hwmGameMode;
		yield return m_GameMode.StartCoroutine(m_GameMode.StartPlay());
	}

	public IEnumerator EndPlay()
	{
		m_GameState.Destroy();
		m_GameState = null;
		UnityEngine.Object.Destroy(m_GameMode);
		UnityEngine.Object.Destroy(m_GameplayFrameworkObject);

		m_Level = null;
		yield return null;
	}

	public hwmGameState GetGameState()
	{
		return m_GameState;
	}
}