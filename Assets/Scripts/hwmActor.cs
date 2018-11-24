using UnityEngine;

public class hwmActor : MonoBehaviour
{
	protected hwmConstants.NetRole m_Role;

	public void Initialize(hwmConstants.NetRole role, object additionalData)
	{
		m_Role = role;

		HandleInitialize(additionalData);
	}

	public void Dispose()
	{
		HandleDispose();
	}

	public bool IsConrtollerByThieDevice()
	{
		return m_Role == hwmConstants.NetRole.Authority
			|| m_Role == hwmConstants.NetRole.AutonomousProxy;
	}

	protected virtual void HandleInitialize(object additionalData)
	{

	}

	protected virtual void HandleDispose()
	{

	}
}