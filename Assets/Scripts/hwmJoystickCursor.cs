using UnityEngine;
using UnityEngine.UI;

public class hwmJoystickCursor : MonoBehaviour
{
	public Canvas MyCanvas;
	public GameObject AnimationRoot;

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

	public bool IsDisplay()
	{
		return gameObject.activeInHierarchy && m_Display;
	}

	public Vector2 GetCusrorPosition_ScreenSpace()
	{
		return m_CursorPosition_ScreenSpace;
	}

	protected void Awake()
	{
		MyCanvas.sortingOrder = (int)snConstants.CanvasSortOrder.JoystickCursor;

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
		bool hasInput = false;

		// has input
		if (axis.sqrMagnitude > Mathf.Epsilon)
		{
			hasInput = true;
			if (!m_Display)
			{
				SetDisplay(true);
			}
			Vector2 position_CanvasSpace = transform.localPosition; // origin in canvas center 
			position_CanvasSpace += axis;
			position_CanvasSpace.x = Mathf.Clamp(position_CanvasSpace.x, -m_CanvasHalfSize.x, m_CanvasHalfSize.x);
			position_CanvasSpace.y = Mathf.Clamp(position_CanvasSpace.y, -m_CanvasHalfSize.y, m_CanvasHalfSize.y);
			transform.localPosition = position_CanvasSpace;

			m_CursorPosition_ScreenSpace = (position_CanvasSpace + m_CanvasHalfSize) // change origin to canvas leftdown
				* m_CanvasToScreenScale;

			m_LastHasInputTime = snSystem.GetInstance().GetRealtimeSinceStartup();
		}
		// hide when no input for a long time
		else if (m_Display)
		{
			float currentTime = snSystem.GetInstance().GetRealtimeSinceStartup();
			if (currentTime - m_LastHasInputTime >= AutoHideCursorTime)
			{
				SetDisplay(false);
			}
			else if (Input.GetButton(SubmitButtonName)) // submit only vaild when display
			{
				hasInput = true;
			}
		}

		if (hasInput)
		{
			m_LastHasInputTime = snSystem.GetInstance().GetRealtimeSinceStartup();
		}

		// play click animation
		if (m_Display
			&& Input.GetButtonUp(SubmitButtonName))
		{
			CursorAnimator.SetBool(CursorAnimatorClickParmeterName, true);
		}
	}

	private void OnClickAnimationPlayBegin()
	{
		CursorAnimator.SetBool(CursorAnimatorClickParmeterName, false);
	}

	private void CalculateMaxCursorPosition()
	{
		CanvasScaler canvasScaler = MyCanvas.GetComponent<CanvasScaler>();

		m_CanvasHalfSize.x = MyCanvas.pixelRect.width / MyCanvas.scaleFactor;
		m_CanvasToScreenScale = Screen.width / m_CanvasHalfSize.x;
		m_CanvasHalfSize.x *= 0.5f;
		m_CanvasHalfSize.y = m_CanvasHalfSize.x / Screen.width * Screen.height;
	}

	private void SetDisplay(bool display)
	{
		m_Display = display;
		AnimationRoot.SetActive(display);

		// stop click animation when hide cursor
		if (!display)
		{
			CursorAnimator.SetBool(CursorAnimatorClickParmeterName, false);
		}
	}

}