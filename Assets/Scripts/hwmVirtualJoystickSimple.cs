using UnityEngine;
using UnityEngine.EventSystems;


public class hwmVirtualJoystickSimple : hwmBaseVirtualJoystick, IDragHandler, IEndDragHandler
{
	public RectTransform CursorRect;
	public float MaxMoveDistance = 50;
	public float MoveSpeed = 5;

	private State m_State;
	private Vector3 m_StartPosition;

	public void OnDrag(PointerEventData eventData)
	{
		m_State = State.Move;
		Vector3 worldPosition;
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(CursorRect, eventData.position, eventData.pressEventCamera, out worldPosition))
		{
			CursorRect.position = worldPosition;
		}

		if ((CursorRect.localPosition - m_StartPosition).sqrMagnitude >= MaxMoveDistance)
		{
			CursorRect.localPosition = m_StartPosition
				+ (CursorRect.localPosition - m_StartPosition).normalized * MaxMoveDistance;
		}

		m_Value = new Vector2(CursorRect.localPosition.x / MaxMoveDistance, CursorRect.localPosition.y / MaxMoveDistance);
		m_ValueAvailable = true;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		m_State = State.Restore;
		m_ValueAvailable = false;
	}

	protected void Awake()
	{
		m_StartPosition = CursorRect.localPosition;
		m_State = State.Stand;
		m_ValueAvailable = false;
	}

	protected void Update()
	{
		if (m_State == State.Restore)
		{
			CursorRect.localPosition = Vector3.MoveTowards(CursorRect.localPosition
				, m_StartPosition
				, MoveSpeed * Time.deltaTime);

			if ((m_StartPosition - CursorRect.localPosition).sqrMagnitude < 0.1f)
			{
				m_State = State.Stand;
			}
		}
	}

	public enum State
	{
		Stand,
		Move,
		Restore
	}
}