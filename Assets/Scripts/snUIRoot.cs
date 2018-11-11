using UnityEngine;

public abstract class snUIRoot : MonoBehaviour
{
	public abstract void OnUIRootInitialize();

	public abstract void OnUIRootDestroy();

	public abstract void OnUIRootDisplay();

	public abstract void OnUIRootHide();

	public virtual void OnLanguageChanged()
	{
	}
}