using System;
using System.Collections;
using UnityEngine;

public class hwmGameMode : MonoBehaviour
{
	public IEnumerator StartPlay_Co()
	{
		yield return StartCoroutine(HandleStartPlay_Co());
		hwmWorld.GetInstance().GetGameState().ChangeMatchState(hwmMatchState.WaitingToStart);
		yield return StartCoroutine(StartMatch_Co());
	}

	public IEnumerator StartMatch_Co()
	{
		yield return StartCoroutine(HandleStartMatch_Co());
		hwmWorld.GetInstance().GetGameState().ChangeMatchState(hwmMatchState.InProgress);
	}

	public IEnumerator EndMatch_Co()
	{
		yield return StartCoroutine(HandleEndMatch_Co());
		hwmWorld.GetInstance().GetGameState().ChangeMatchState(hwmMatchState.WaitingPostMatch);
	}

	public IEnumerator EndPlay_Co()
	{
		yield return StartCoroutine(HandleEndPlay_Co());
		hwmWorld.GetInstance().GetGameState().ChangeMatchState(hwmMatchState.Aborted);
	}

	protected virtual IEnumerator HandleEndMatch_Co()
	{
		yield return null;
	}

	protected virtual IEnumerator HandleEndPlay_Co()
	{
		yield return null;
	}

	protected virtual IEnumerator HandleStartPlay_Co()
	{
		yield return null;
	}

	protected virtual IEnumerator HandleStartMatch_Co()
	{
		yield return null;
	}
}