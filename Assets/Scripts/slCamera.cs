using UnityEngine;

public class slCamera : MonoBehaviour
{
	public SizeProperties NormalSizeProperties;

	private Camera m_Camera;

	private SizeProperties m_CurrentSizeProperties;

	private RadarState m_RadarState;
	private SizeProperties m_RadarStateSizeProperties;

	public void OnSetControllerSnake()
	{
		m_RadarState = RadarState.None;
	}

	public void OnUnControllerSnake()
	{
		m_RadarState = RadarState.None;
	}

	public void EnableRadarState(bool enable, SizeProperties sizeProperties)
	{
		m_RadarState = enable ? RadarState.Enter : RadarState.Exit;
		m_RadarStateSizeProperties = sizeProperties;
	}

	protected void Awake()
	{
		m_Camera = gameObject.GetComponent<Camera>();
		m_Camera.orthographicSize = NormalSizeProperties.Destination;
	}

	protected void OnDestroy()
	{
		m_Camera = null;
	}

	protected void Update()
	{
		if (m_RadarState == RadarState.Enter)
		{
			m_CurrentSizeProperties = m_RadarStateSizeProperties;
		}
		else if (m_RadarState == RadarState.Enter)
		{
			m_CurrentSizeProperties.Destination = NormalSizeProperties.Destination;
			m_CurrentSizeProperties.LerpSpeed = m_RadarStateSizeProperties.LerpSpeed;
		}
		else
		{
			m_CurrentSizeProperties = NormalSizeProperties;
		}

		m_Camera.orthographicSize = Mathf.Lerp(m_Camera.orthographicSize
			, m_CurrentSizeProperties.Destination
			, Time.deltaTime * m_CurrentSizeProperties.LerpSpeed);

		if (m_RadarState == RadarState.Exit
			&& Mathf.Approximately(m_Camera.orthographicSize, m_CurrentSizeProperties.Destination))
		{
			m_RadarState = RadarState.None;
		}
	}

	[System.Serializable]
	public struct SizeProperties
	{
		public float Destination;
		public float LerpSpeed;
	}

	public enum RadarState
	{
		None,
		Enter,
		Exit,
	}
}