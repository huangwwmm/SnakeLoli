﻿using UnityEngine;

public class hwmDebugView : MonoBehaviour
{
	private const int FTP_UPDATE_INTERVALFRAME = 180;

	public Font UIFont;

	private float m_FPS;
	private float m_LastUpdateFPSTime = 0;
	private int m_UpdateFPSFrames = 0;

	protected void Update()
	{
		if (++m_UpdateFPSFrames == FTP_UPDATE_INTERVALFRAME)
		{
			float realtimeSinceStartup = hwmSystem.GetInstance().GetRealtimeSinceStartup();
			m_FPS = 1.0f * m_UpdateFPSFrames / (realtimeSinceStartup - m_LastUpdateFPSTime);

			m_LastUpdateFPSTime = realtimeSinceStartup;
			m_UpdateFPSFrames = 0;
		}
	}

	protected void OnGUI()
	{
		GUI.skin.font = UIFont;

		GUILayout.Box(string.Format("FPS: {0:F2}", m_FPS));
	}
}