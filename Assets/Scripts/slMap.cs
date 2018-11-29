using UnityEngine;

public class slMap : MonoBehaviour
{
	private slMapPresentation m_Presentation;
	private GameObject m_WallGameObject;
	private BoxCollider2D[] m_Walls;
	private hwmBounds2D m_MapBounds;

	public void Initialize(Vector2 mapSize)
	{
		m_MapBounds = new hwmBounds2D(Vector2.zero, mapSize);

		// Wall
		m_WallGameObject = new GameObject("Wall");
		m_WallGameObject.transform.SetParent(transform, false);
		m_WallGameObject.layer = (int)slConstants.Layer.Wall;

		BoxCollider2D up = m_WallGameObject.AddComponent<BoxCollider2D>();
		BoxCollider2D down = m_WallGameObject.AddComponent<BoxCollider2D>();
		BoxCollider2D left = m_WallGameObject.AddComponent<BoxCollider2D>();
		BoxCollider2D right = m_WallGameObject.AddComponent<BoxCollider2D>();

		up.size = new Vector2(mapSize.x + slConstants.WALL_THICK * 2.0f, slConstants.WALL_THICK);
		down.size = new Vector2(mapSize.x + slConstants.WALL_THICK * 2.0f, slConstants.WALL_THICK);
		left.size = new Vector2(slConstants.WALL_THICK, mapSize.y + slConstants.WALL_THICK * 2.0f);
		right.size = new Vector2(slConstants.WALL_THICK, mapSize.y + slConstants.WALL_THICK * 2.0f);
		up.offset = new Vector2(0, (mapSize.y + slConstants.WALL_THICK) * 0.5f);
		down.offset = new Vector2(0, -(mapSize.y + slConstants.WALL_THICK) * 0.5f);
		left.offset = new Vector2(-(mapSize.x + slConstants.WALL_THICK) * 0.5f, 0);
		right.offset = new Vector2((mapSize.x + slConstants.WALL_THICK) * 0.5f, 0);

		m_Walls = new BoxCollider2D[4];
		m_Walls[0] = up;
		m_Walls[1] = down;
		m_Walls[2] = left;
		m_Walls[3] = right;

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

		// Wall
		Destroy(m_WallGameObject);
		m_Walls = null;
		m_WallGameObject = null;
	}

	public hwmBounds2D GetMapBounds()
	{
		return m_MapBounds;
	}
}