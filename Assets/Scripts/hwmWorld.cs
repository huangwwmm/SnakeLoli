using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// TODO support multiplayer
/// </summary>
public class hwmWorld
{
	protected static hwmWorld ms_Instance;

	protected hwmLevel m_Level;
	protected GameObject m_GameplayFrameworkObject;
	protected hwmGameState m_GameState;
	protected hwmGameMode m_GameMode;

	public static hwmWorld GetInstance()
	{
		return ms_Instance;
	}

	public hwmWorld()
	{
		ms_Instance = this;
	}

	public IEnumerator BeginPlay_Co()
	{
		m_Level = hwmSystem.GetInstance().GetWaitingToPlayLevel();
		hwmDebug.Assert(m_Level != null, "m_Level != null");

		m_GameplayFrameworkObject = new GameObject("Gameplay Framework");

		m_GameState = Activator.CreateInstance(Type.GetType(m_Level.GameStateClassName)) as hwmGameState;
		m_GameState.Initialize();
		m_GameMode = m_GameplayFrameworkObject.AddComponent(Type.GetType(m_Level.GameModeClassName)) as hwmGameMode;
		yield return m_GameMode.StartCoroutine(HandleBeginPlay_Co());
		yield return m_GameMode.StartCoroutine(m_GameMode.StartPlay_Co());
	}

	public IEnumerator EndPlay_Co()
	{
		yield return m_GameMode.StartCoroutine(HandleEndPlay_Co());

		m_GameState.Dispose();
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

	public hwmLevel GetLevel()
	{
		return m_Level;
	}

	public hwmActor CreateActor(string name, string prefabName, Vector3 position, Quaternion rotation, object additionalData = null)
	{
		GameObject actorPrefab = hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Actor, prefabName) as GameObject;

		GameObject actorGameObject = UnityEngine.Object.Instantiate(actorPrefab, position, rotation) as GameObject;
		actorGameObject.name = name;
		hwmActor actor = actorGameObject.GetComponent(typeof(hwmActor)) as hwmActor;
		actor.Initialize(hwmConstants.NetRole.Authority, additionalData);
		return actor;
	}

	public bool NeedPresentation()
	{
		return true;
	}

	protected virtual IEnumerator HandleBeginPlay_Co()
	{
		yield return null;
	}

	protected virtual IEnumerator HandleEndPlay_Co()
	{
		yield return null;
	}
}