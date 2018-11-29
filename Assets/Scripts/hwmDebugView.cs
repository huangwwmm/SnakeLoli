using UnityEngine;

public class hwmDebugView : MonoBehaviour
{
	private const int FPS_UPDATE_INTERVALFRAME = 60;
	private const int FPSAVG_UPDATE_SAMPLE = 10;

	public Font UIFont;

	private float m_FPS;
	private float m_LastUpdateFPSTime = 0;
	private int m_UpdateFPSFrames = 0;
	private float m_FPSAvg = 0;
	private float m_LastFPSAvg = 0;
	private int m_FPSAvgSample = 0;

	protected void Update()
	{
		if (++m_UpdateFPSFrames == FPS_UPDATE_INTERVALFRAME)
		{
			float realtimeSinceStartup = hwmSystem.GetInstance().GetRealtimeSinceStartup();
			m_FPS = 1.0f * m_UpdateFPSFrames / (realtimeSinceStartup - m_LastUpdateFPSTime);

			m_FPSAvg = ((m_FPSAvg * m_FPSAvgSample++) + m_FPS) / m_FPSAvgSample;
			if (m_FPSAvgSample == FPSAVG_UPDATE_SAMPLE)
			{
				m_LastFPSAvg = m_FPS;
				m_FPSAvg = 0;
				m_FPSAvgSample = 0;
			}

			m_LastUpdateFPSTime = realtimeSinceStartup;
			m_UpdateFPSFrames = 0;
		}
	}

	protected void OnGUI()
	{
		GUI.skin.font = UIFont;

		GUILayout.Box(string.Format("FPS: {0:F2}", m_FPS));
		GUILayout.Box(string.Format("FPS Avg: {0:F2}", m_LastFPSAvg));
	}
}