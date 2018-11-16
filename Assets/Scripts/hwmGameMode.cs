using System.Collections;
using UnityEngine;

public class hwmGameMode : MonoBehaviour
{
	protected hwmLevel m_Level;
	protected hwmGameState m_GameState;

	public IEnumerator InitGame()
	{
		m_Level = hwmSystem.GetInstance().GetWorld().GetLevel();
		m_GameState = hwmSystem.GetInstance().GetWorld().GetGameState();

		m_GameState.ChangeMatchState(hwmMatchState.EnteringMap);
		yield return StartCoroutine(HandleInitGame());
		yield return StartCoroutine(StartPlay());
	}

	public IEnumerator StartPlay()
	{
		yield return StartCoroutine(HandleStartPlay());
		m_GameState.ChangeMatchState(hwmMatchState.WaitingToStart);
		yield return StartCoroutine(StartMatch());
	}

	public IEnumerator StartMatch()
	{
		yield return StartCoroutine(HandleStartMatch());
		m_GameState.ChangeMatchState(hwmMatchState.InProgress);
	}

	protected virtual IEnumerator HandleInitGame()
	{
		yield return null;
	}

	protected virtual IEnumerator HandleStartPlay()
	{
		yield return null;
	}

	protected virtual IEnumerator HandleStartMatch()
	{
		yield return null;
	}
}