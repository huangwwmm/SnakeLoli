using UnityEngine;

public class Test : MonoBehaviour 
{
	//public Vector2 center;
	//public Vector2 size;
	//public float angle;

	//public GameObject go;
	//public bool cast;

	//private void Update()
	//{
	//	RaycastHit2D hit = Physics2D.BoxCast(center, size, angle, Vector2.zero);
	//	cast = hit.collider != null;

	//	go.transform.position = center;
	//	go.transform.localScale = new Vector3(size.x, size.y, 1);
	//	go.transform.eulerAngles = new Vector3(0, 0, angle);
	//}

	public Vector2 from;
	public Vector2 to;
	public float angle;

	public GameObject f;
	public GameObject t;
	public GameObject r;

	private void Update()
	{
		//f.transform.position = from;
		//t.transform.position = to;

		//r.transform.position = hwmUtility.CircleLerp(from, to, angle);

		f.transform.rotation = Quaternion.Euler(0, 0, -Vector2.SignedAngle(from, Vector2.up));
	}
}