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

	private Vector2 m_CanvasHalfSize;
	/// <summary>
	/// Cursor not always display. It display when has input, and hide when no input for a long time
	/// </summary>
	private bool m_Display = false;
	private float m_LastDisplayTime = 0.0f;

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

		Vector3 axis = new Vector3(Input.GetAxis(XInputAxesName) * CursorMoveSpeed
			, Input.GetAxis(YInputAxesName) * CursorMoveSpeed
			, 0.0f);

		if (axis.sqrMagnitude > Mathf.Epsilon)
		{
			if (!m_Display)
			{
				SetDisplay(true);
			}
			Vector3 position = transform.localPosition;
			position += axis;
			position.x = Mathf.Clamp(position.x, -m_CanvasHalfSize.x, m_CanvasHalfSize.x);
			position.y = Mathf.Clamp(position.y, -m_CanvasHalfSize.y, m_CanvasHalfSize.y);
			transform.localPosition = position;
		}
		else
		{

		}

		if (m_Display
			&& Input.GetButtonUp("Submit"))
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

		m_CanvasHalfSize.x = MyCanvas.pixelRect.width / MyCanvas.scaleFactor * 0.5f;
		m_CanvasHalfSize.y = m_CanvasHalfSize.x / Screen.width * Screen.height;
	}

	private void SetDisplay(bool display)
	{
		m_Display = display;
		AnimationRoot.SetActive(display);

		if (display)
		{
			m_LastDisplayTime = Time.time;
		}
		else
		{
			CursorAnimator.SetBool(CursorAnimatorClickParmeterName, false);
		}
	}

}