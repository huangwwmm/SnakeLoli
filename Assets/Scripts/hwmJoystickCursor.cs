using UnityEngine;

public class hwmJoystickCursor : MonoBehaviour
{
	private const float SPEED_MULIT_WHEN_POINTER_OVER_GAMEOBJECT = 0.7f;
	private const float AUTOHIDETIEM_MULIT_WHEN_POINTER_OVER_GAMEOBJECT = 1.5f;

	public Canvas MyCanvas;
	public GameObject Cursor;

	public Animator CursorAnimator;
	public string CursorAnimatorClickParmeterName;
	public string CursorAnimatorHoverParmeterName;

	public string XInputAxesName;
	public string YInputAxesName;

	[Range(0.0f, 99.0f)]
	public float CursorMoveSpeed = 10.0f;
	[Range(0.0f, 99.0f)]
	public float AutoHideCursorTime = 3.0f;
	public string SubmitButtonName = "Submit";

	private Vector2 m_CanvasHalfSize;
	private float m_CanvasToScreenScale;
	/// <summary>
	/// Cursor not always display. It display when has input, and hide when no input for <see cref="AutoHideCursorTime"/>
	/// </summary>
	private bool m_Display = false;
	private float m_LastHasInputTime = 0.0f;
	/// <summary>
	/// only valid when cursor display
	/// </summary>
	private Vector2 m_CursorPosition_ScreenSpace;
	private PressState m_PressState = PressState.Notset;

	public bool IsDisplay()
	{
		return m_Display;
	}

	public PressState GetPressState()
	{
		return m_PressState;
	}

	public Vector2 GetCusrorPosition_ScreenSpace()
	{
		return m_CursorPosition_ScreenSpace;
	}

	protected void Awake()
	{
		MyCanvas.sortingOrder = hwmConstants.CANVAS_SORTORDER_JOYSTICKCURSOR;

		CalculateMaxCursorPosition();
		SetDisplay(false);
	}

	protected void OnDestroy()
	{
	}

	protected void Update()
	{
		Vector2 axis = new Vector2(Input.GetAxis(XInputAxesName) * CursorMoveSpeed
			, Input.GetAxis(YInputAxesName) * CursorMoveSpeed);
		bool isPointerOverGameObject = hwmSystem.GetInstance().GetInput().EventSystem.IsPointerOverGameObject();
		if (isPointerOverGameObject)
		{
			axis *= SPEED_MULIT_WHEN_POINTER_OVER_GAMEOBJECT;
		}
		bool hasInput = false;

		// has input
		if (axis.sqrMagnitude > Mathf.Epsilon)
		{
			hasInput = true;
			if (!m_Display)
			{
				SetDisplay(true);
			}

			Vector2 position_CanvasSpace = Cursor.transform.localPosition; // origin in canvas center 
			position_CanvasSpace += axis;
			position_CanvasSpace.x = Mathf.Clamp(position_CanvasSpace.x, -m_CanvasHalfSize.x, m_CanvasHalfSize.x);
			position_CanvasSpace.y = Mathf.Clamp(position_CanvasSpace.y, -m_CanvasHalfSize.y, m_CanvasHalfSize.y);
			Cursor.transform.localPosition = position_CanvasSpace;

			m_CursorPosition_ScreenSpace = (position_CanvasSpace + m_CanvasHalfSize) // change origin to canvas leftdown
				* m_CanvasToScreenScale;

			m_LastHasInputTime = hwmSystem.GetInstance().GetRealtimeSinceStartup();
		}
		// hide when no input for a long time
		else if (m_Display)
		{
			float currentTime = hwmSystem.GetInstance().GetRealtimeSinceStartup();
			if (currentTime - m_LastHasInputTime >= (isPointerOverGameObject ? AUTOHIDETIEM_MULIT_WHEN_POINTER_OVER_GAMEOBJECT : 1.0f) * AutoHideCursorTime)
			{
				SetDisplay(false);
			}
		}

		// play click animation
		if (m_Display)
		{
			if (Input.GetButtonUp(SubmitButtonName))
			{
				hasInput = true;
				m_PressState = PressState.Up;
				CursorAnimator.SetBool(CursorAnimatorClickParmeterName, true);
			}
			else if (Input.GetButton(SubmitButtonName))
			{
				hasInput = true;
				m_PressState = m_PressState == PressState.Notset
					? PressState.Down
					: PressState.Hold;
			}
			else
			{
				m_PressState = PressState.Notset;
			}

			CursorAnimator.SetBool(CursorAnimatorHoverParmeterName, isPointerOverGameObject);
		}
		else
		{
			// handle on SetDisplay()
		}

		if (hasInput)
		{
			m_LastHasInputTime = hwmSystem.GetInstance().GetRealtimeSinceStartup();
		}
	}

	private void OnClickAnimationPlayBegin()
	{
		CursorAnimator.SetBool(CursorAnimatorClickParmeterName, false);
	}

	private void CalculateMaxCursorPosition()
	{
		m_CanvasHalfSize.x = MyCanvas.pixelRect.width / MyCanvas.scaleFactor;
		m_CanvasToScreenScale = Screen.width / m_CanvasHalfSize.x;
		m_CanvasHalfSize.x *= 0.5f;
		m_CanvasHalfSize.y = m_CanvasHalfSize.x / Screen.width * Screen.height;
	}

	private void SetDisplay(bool display)
	{
		m_Display = display;
		Cursor.SetActive(display);

		// stop click animation when hide cursor
		if (!display)
		{
			CursorAnimator.SetBool(CursorAnimatorClickParmeterName, false);
			m_PressState = PressState.Notset;
		}
	}

	public enum PressState : byte
	{
		Notset,
		Down,
		Hold,
		Up,
	}
}