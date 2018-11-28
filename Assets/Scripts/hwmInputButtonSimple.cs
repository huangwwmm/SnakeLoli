using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class hwmInputButtonSimple : hwmBaseInputButton, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
	private Button m_UIButton;
	private bool m_IsDown;

	public void OnPointerDown(PointerEventData eventData)
	{
		m_Press = true;
		m_IsDown = true;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (m_IsDown)
		{
			m_Press = true;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		m_Press = false;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		m_Press = false;
		m_IsDown = false;
	}

	public Button GetUIButton()
	{
		return m_UIButton;
	}

	protected void OnEnable()
	{
		m_IsDown = false;
	}

	protected void Awake()
	{
		m_UIButton = gameObject.GetComponent<Button>();
	}

	protected void OnDestroy()
	{
		m_UIButton = null;
	}
}