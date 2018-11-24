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
			case hwmMatchState.WaitingToStart:
				HandleWaitingToStart();
				break;
			case hwmMatchState.InProgress:
				HandleInProgress();
				break;
			case hwmMatchState.WaitingPostMatch:
				HandleWaitingPostMatch();
				break;
			case hwmMatchState.Aborted:
				HandleAborted();
				break;
		}
	}

	public List<hwmPlayerState> GetPlayerStates()
	{
		return m_PlayerStates;
	}

	public hwmMatchState GetMatchState()
	{
		return m_MatchState;
	}

	public hwmPlayerState FindPlayerStateByPlayerID(int playerID)
	{
		for (int iPlayer = 0; iPlayer < m_PlayerStates.Count; iPlayer++)
		{
			if (m_PlayerStates[iPlayer].PlayerID == playerID)
			{
				return m_PlayerStates[iPlayer];
			}
		}
		return null;
	}

	public void AddPlayerState(hwmPlayerState playerState)
	{
		m_PlayerStates.Add(playerState);
	}

	protected virtual void HandleAborted()
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
}