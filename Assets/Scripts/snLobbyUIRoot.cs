using UnityEngine.UI;

public class snLobbyUIRoot : snUIRoot
{
	private Button m_StartFreeGameButton;
	private Text m_StartFreeGameText;

	public override void OnUIRootInitialize()
	{
	}

	public override void OnUIRootDestroy()
	{
	}

	public override void OnUIRootDisplay()
	{
		m_StartFreeGameButton.onClick.AddListener(On_StartFreeGameButton);
	}

	public override void OnUIRootHide()
	{
		m_StartFreeGameButton.onClick.RemoveAllListeners();
	}

	public override void OnLanguageChanged()
	{
	}

	private void On_StartFreeGameButton()
	{

	}
}