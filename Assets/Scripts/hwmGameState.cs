using System;
using System.Collections.Generic;

public class hwmGameState 
{
	private hwmMatchState m_MatchState;
	private List<hwmPlayerState> m_PlayerStates;

	public void Initialize()
	{
		m_PlayerStates = new List<hwmPlayerState>();
	}

	public void Dispose()
	{
		m_PlayerStates.Clear();
		m_PlayerStates = null;
	}

	public void ChangeMatchState(hwmMatchState matchState)
	{
		m_MatchState = matchState;

		switch(matchState)
		{
			case hwmMatchState.EnteringMap:
				HandleEnteringMap();
				break;
			case hwmMatchState.WaitingToStart:
				HandleWaitingToStart();
				break;
			case hwmMatchState.InProgress:
				HandleInProgress();
				break;
			case hwmMatchState.WaitingPostMatch:
				HandleWaitingPostMatch();
				break;
			case hwmMatchState.LeavingMap:
				HandleLeavingMap();
				break;
			case hwmMatchState.Aborted:
				HandleAborted();
				break;
		}
	}

	public hwmMatchState GetMatchState()
	{
		return m_MatchState;
	}

	protected virtual void HandleAborted()
	{
	}

	protected virtual void HandleLeavingMap()
	{
	}

	protected virtual void HandleWaitingPostMatch()
	{
	}

	protected virtual void HandleInProgress()
	{
	}

	protected virtual void HandleWaitingToStart()
	{
	}

	protected virtual void HandleEnteringMap()
	{
	}
}