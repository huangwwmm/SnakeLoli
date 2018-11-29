using UnityEngine;

public class hwmActor : MonoBehaviour
{
	protected hwmConstants.NetRole m_Role;
	protected string m_GuidStr;
	protected int m_Guid;

	public void Initialize(hwmConstants.NetRole role, int guid, object additionalData)
	{
		m_Role = role;
		m_Guid = guid;
		m_GuidStr = m_Guid.ToString();
		HandleInitialize(additionalData);

		hwmObserver.NotifyActorCreate(this);
	}

	public void Dispose(object additionalData)
	{
		hwmObserver.NotifyActorDestroy(this);

		HandleDispose(additionalData);
	}

	public int GetGuid()
	{
		return m_Guid;
	}

	public string GetGuidStr()
	{
		return m_GuidStr;
	}

	protected virtual void HandleInitialize(object additionalData)
	{

	}

	protected virtual void HandleDispose(object additionalData)
	{

	}
}