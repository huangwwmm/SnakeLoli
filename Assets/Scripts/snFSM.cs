using UnityEngine;
using System.Collections;

public class snFSM : MonoBehaviour
{
	private snFSMState[] m_States;

	private snFSMState m_CurrentState = null;
	private bool m_IsChanging = false;
	private snFSMState m_ChangeFromState = null;
	private snFSMState m_ChangeToState = null;

	public snFSMState FindState(string name)
	{
		for (int iState = 0; iState < m_States.Length; iState++)
		{
			snFSMState iterState = m_States[iState];
			if (iterState.StateName == name)
			{
				return iterState;
			}
		}
		return null;
	}

	public void ChangeState_Instantly(snFSMState newState)
	{
		ChangeStart(newState);

		if (m_ChangeFromState != null)
		{
			snDebug.Assert(m_ChangeFromState.SupportInstantlyChange(), "ChangeFromState not support instantly change");
			m_ChangeFromState.Deactivate_Instantly();
		}

		m_CurrentState = null;

		if (m_ChangeToState != null)
		{
			snDebug.Assert(m_ChangeFromState.SupportInstantlyChange(), "m_ChangeToState not support instantly change");
			m_ChangeToState.Activate_Instantly();
		}

		ChangeFinished();
	}

	public IEnumerator ChangeState_Co(snFSMState newState)
	{
		ChangeStart(newState);

		if (m_ChangeFromState != null)
		{
			snDebug.Assert(m_ChangeFromState.SupportCoroutineChange(), "ChangeFromState not support coroutine change");
			yield return StartCoroutine(m_ChangeFromState.Deactivate_Co());
		}

		m_CurrentState = null;

		if (m_ChangeToState != null)
		{
			snDebug.Assert(m_ChangeToState.SupportCoroutineChange(), "m_ChangeToState not support coroutine change");
			yield return StartCoroutine(m_ChangeToState.Activate_Co());
		}

		ChangeFinished();
	}

	public bool IsChanging()
	{
		return m_IsChanging;
	}

	protected virtual void Awake()
	{
		m_States = GetComponentsInChildren<snFSMState>(true);
		for (int iState = 0; iState < m_States.Length; iState++)
		{
			snFSMState iterState = m_States[iState];
			snDebug.Assert(iterState.SupportInstantlyChange() || iterState.SupportCoroutineChange()
				, string.Format("State ({0}) not support change", iterState.StateName));
			iterState.Initialize(this);
		}
	}

	protected virtual void OnDestroy()
	{
		m_States = null;
		m_CurrentState = null;
		m_ChangeFromState = null;
		m_ChangeToState = null;
	}

	private void ChangeStart(snFSMState newState)
	{
		snDebug.Assert(!m_IsChanging, "!m_IsChanging");

		Debug.Log(string.Format("Change state from ({0}) to ({1}) start"
			, m_CurrentState == null ? "NULL" : m_CurrentState.StateName
			, newState == null ? "NULL" : newState.StateName));

		m_IsChanging = true;
		m_ChangeFromState = m_CurrentState;
		m_ChangeToState = newState;
	}

	private void ChangeFinished()
	{
		snDebug.Assert(m_IsChanging, "m_IsChanging");

		Debug.Log(string.Format("Change state from ({0}) to ({1}) finished"
			, m_ChangeFromState == null ? "NULL" : m_ChangeFromState.StateName
			, m_ChangeToState == null ? "NULL" : m_ChangeToState.StateName));

		m_CurrentState = m_ChangeToState;

		m_IsChanging = false;
		m_ChangeFromState = null;
		m_ChangeToState = null;
	}
}