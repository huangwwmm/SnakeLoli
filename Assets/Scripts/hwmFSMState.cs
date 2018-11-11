using System.Collections;
using UnityEngine;

public class hwmFSMState : MonoBehaviour
{
	public string StateName;

	public virtual void Initialize(hwmFSM owner)
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