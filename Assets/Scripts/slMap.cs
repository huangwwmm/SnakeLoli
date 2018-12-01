using UnityEngine;

public class slMap : MonoBehaviour
{
	private slMapPresentation m_Presentation;
	private hwmBox2D m_MapBox;

	public void Initialize(Vector2 mapSize)
	{
		m_MapBox = hwmBox2D.BuildAABB(Vector2.zero, mapSize * 0.5f);

		// Presentation
		if (hwmWorld.GetInstance().NeedPresentation())
		{
			m_Presentation = (Instantiate(hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Game, "MapPresentation")) as GameObject).GetComponent<slMapPresentation>();
			m_Presentation.transform.SetParent(transform, false);
			m_Presentation.Initialize(this);
		}
	}

	public void Dispose()
	{
		// Presentation
		if (m_Presentation)
		{
			m_Presentation.Dispose();
			Destroy(m_Presentation.gameObject);
			m_Presentation = null;
		}
	}

	public hwmBox2D GetMapBox()
	{
		return m_MapBox;
	}
}