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

	protected virtual IEnumerator HandleStartPlay_Co()
	{
		yield return null;
	}

	protected virtual IEnumerator HandleStartMatch_Co()
	{
		yield return null;
	}
}