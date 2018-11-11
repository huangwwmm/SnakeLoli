using UnityEngine;

public class hwmDestroyImmediateAtAwake : MonoBehaviour
{
	protected void Awake()
	{
		DestroyImmediate(gameObject);
	}
}