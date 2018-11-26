using UnityEngine;
using UnityEngine.EventSystems;

public class hwmInputVirtualJoystickMobile : hwmBaseInputVirtualJoystick, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public RectTransform JoystickRect;
	public RectTransform CursorRect;
	public float MaxMoveDistance = 50;

	private Vector3 m_CursorWorldPosition;

	private void Awake()
	{
		CursorRect.transform.localPosition = Vector3.zero;
		JoystickRect.gameObject.SetActive(false);
		m_ValueAvailable = false;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		CalculateWorldPosition(eventData);
		JoystickRect.position = m_CursorWorldPosition;
		CursorRect.transform.localPosition = Vector3.zero;
		JoystickRect.gameObject.SetActive(true);
	}

	public void OnDrag(PointerEventData eventData)
	{
		CalculateWorldPosition(eventData);

		CursorRect.position = m_CursorWorldPosition;

		if (Vector3.Distance(CursorRect.localPosition, Vector3.zero) >= MaxMoveDistance)
		{
			CursorRect.localPosition = CursorRect.localPosition.normalized * MaxMoveDistance;
		}

		m_Value = new Vector2(CursorRect.localPosition.x / MaxMoveDistance, CursorRect.localPosition.y / MaxMoveDistance);
		m_ValueAvailable = true;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		JoystickRect.gameObject.SetActive(false);
		m_ValueAvailable = false;
	}

	void CalculateWorldPosition(PointerEventData eventData)
	{
		RectTransformUtility.ScreenPointToWorldPointInRectangle(JoystickRect, eventData.position, eventData.pressEventCamera, out m_CursorWorldPosition);
	}
}