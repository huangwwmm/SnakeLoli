using System.Collections;
using UnityEngine;

public class snFSMState : MonoBehaviour
{
	public virtual void Initialize(snFSM owner)
	{
	}

	#region Instantly
	public virtual bool SupportInstantlyChange()
	{
		return false;
	}

	public virtual void Activate_Instantly()
	{
	}

	public virtual void Deactivate_Instantly()
	{
	}
	#endregion // Instantly

	#region Coroutine
	public virtual bool SupportCoroutineChange()
	{
		return false;
	}

	public virtual IEnumerator Activate_Co()
	{
		yield return null;
	}

	public virtual IEnumerator Deactivate_Co()
	{
		yield return null;
	}
	#endregion // Coroutine
}