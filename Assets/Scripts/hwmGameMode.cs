using System.Collections;
using UnityEngine;

public class hwmGameMode : MonoBehaviour
{
	private hwmGameState m_GameState;

	public IEnumerator StartPlay()
	{
		m_GameState = hwmSystem.GetInstance().GetWorld().GetGameState();

		m_GameState.ChangeMatchState(hwmMatchState.EnteringMap);
		yield return StartCoroutine(HandleStartPlay());
		m_GameState.ChangeMatchState(hwmMatchState.WaitingToStart);
		yield return StartCoroutine(StartMatch());
	}

	public IEnumerator StartMatch()
	{
		yield return StartCoroutine(HandleStartMatch());
		m_GameState.ChangeMatchState(hwmMatchState.InProgress);
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