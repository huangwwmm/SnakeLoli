﻿using System;
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
		yield return m_GameMode.StartCoroutine(EndBeginPlay_Co());
		
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

	protected virtual IEnumerator HandleBeginPlay_Co()
	{
		yield return null;
	}

	protected virtual IEnumerator EndBeginPlay_Co()
	{
		yield return null;
	}
}