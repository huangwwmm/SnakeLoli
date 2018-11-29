using UnityEngine;

public class slMapPresentation : MonoBehaviour
{
	public SpriteRenderer Background;
	public SpriteRenderer Mapground;
	public SpriteRenderer Line;
	public Vector2 LineSpriteSize;
	public float CellSize;

	private slMap m_Owner;
	private GameObject m_LineRoot;

	public void Initialize(slMap map)
	{
		m_Owner = map;

		hwmBounds2D mapBounds = m_Owner.GetMapBounds();
		Vector3 mapSize = mapBounds.size;
		Background.transform.localScale = new Vector3(mapSize.x + 1000.0f, mapSize.y + 1000.0f, 1);
		Mapground.transform.localScale = new Vector3(mapSize.x, mapSize.y, 1);

		m_LineRoot = new GameObject("LineRoot");
		m_LineRoot.transform.SetParent(transform, false);
		CreateLine(Vector2.zero, Vector2.up);
		CreateLine(Vector2.zero, Vector2.down);
		CreateLine(Vector2.zero, Vector2.left);
		CreateLine(Vector2.zero, Vector2.right);
		Line.gameObject.SetActive(false);
	}

	public void Dispose()
	{
		m_Owner = null;
	}

	private void CreateLine(Vector2 startPoint, Vector2 direction)
	{
		hwmBounds2D mapBounds = m_Owner.GetMapBounds();
		Vector2 mapSize = mapBounds.size;

		Vector2 currentPoint = startPoint;
		while (mapBounds.Contains(currentPoint))
		{
			GameObject go = Instantiate(Line.gameObject);
			go.transform.parent = m_LineRoot.transform;
			go.transform.localScale = direction.x == 0
				? new Vector3(mapSize.x / LineSpriteSize.x * 100.0f, 1, 1)
				: new Vector3(1, mapSize.y / LineSpriteSize.y * 100.0f, 1);
			go.transform.localPosition = currentPoint;
			currentPoint += direction * CellSize;
		}
	}
}