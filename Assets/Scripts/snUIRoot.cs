using UnityEngine;

public abstract class snUIRoot : MonoBehaviour
{
	/// <summary>
	/// {0} => Localization key
	/// </summary>
	private const string LOCALIZATION_PLACEHOLDER = "${0}$";

	public abstract void OnUIRootInitialize();

	public abstract void OnUIRootDestroy();

	public abstract void OnUIRootDisplay();

	public abstract void OnUIRootHide();

	public virtual void OnLanguageChanged()
	{
	}
}