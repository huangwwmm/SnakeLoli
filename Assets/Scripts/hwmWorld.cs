using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// TODO support multiplayer
/// </summary>
public class hwmWorld : MonoBehaviour
{
	protected static hwmWorld ms_Instance;

	protected hwmLevel m_Level;
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
		yield return StartCoroutine(HandleBeforeBeginPlay_Co());

		m_Level = hwmSystem.GetInstance().GetWaitingToPlayLevel();
		hwmDebug.Assert(m_Level != null, "m_Level != null");

		m_GameState = Activator.CreateInstance(Type.GetType(m_Level.GameStateClassName)) as hwmGameState;
		m_GameState.Initialize();
		m_GameMode = gameObject.AddComponent(Type.GetType(m_Level.GameModeClassName)) as hwmGameMode;

		yield return StartCoroutine(HandleAfterBeginPlay_Co());
		yield return StartCoroutine(m_GameMode.StartPlay_Co());
	}

	public IEnumerator EndPlay_Co()
	{
		yield return m_GameMode.StartCoroutine(HandleBeforeEndPlay_Co());

		m_GameState.Dispose();
		m_GameState = null;
		Destroy(m_GameMode);
		m_GameMode = null;

		m_Level = null;
		yield return m_GameMode.StartCoroutine(HandleAfterEndPlay_Co());
	}

	public hwmGameState GetGameState()
	{
		return m_GameState;
	}

	public hwmGameMode GetGameMode()
	{
		return m_GameMode;
	}

	public hwmLevel GetLevel()
	{
		return m_Level;
	}

	public hwmActor CreateActor(string name,int guid, string prefabName, Vector3 position, Quaternion rotation, object additionalData = null)
	{
		GameObject actorPrefab = hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Actor, prefabName) as GameObject;

		GameObject actorGameObject = UnityEngine.Object.Instantiate(actorPrefab, position, rotation) as GameObject;
		actorGameObject.name = name;
		hwmActor actor = actorGameObject.GetComponent(typeof(hwmActor)) as hwmActor;
		actor.Initialize(hwmConstants.NetRole.Authority, guid, additionalData);
		return actor;
	}

	public void DestroyActor(hwmActor actor, object additionalData)
	{
		actor.Dispose(additionalData);
		UnityEngine.Object.Destroy(actor.gameObject);
	}

	public bool NeedPresentation()
	{
		return true;
	}

	protected virtual IEnumerator HandleAfterBeginPlay_Co()
	{
		yield return null;
	}

	protected virtual IEnumerator HandleBeforeEndPlay_Co()
	{
		yield return null;
	}

	protected virtual IEnumerator HandleAfterEndPlay_Co()
	{
		yield return null;
	}

	protected virtual IEnumerator HandleBeforeBeginPlay_Co()
	{
		yield return null;
	}
}