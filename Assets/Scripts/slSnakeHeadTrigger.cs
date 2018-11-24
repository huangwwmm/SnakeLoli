using System;
using UnityEngine;

public class slSnakeHeadTrigger : MonoBehaviour
{
	public Action<Collider2D> OnTriggerEnter;

	private void OnTriggerEnter2D(Collider2D collider)
	{
		if (OnTriggerEnter != null
			&& transform.parent != collider.transform.parent)
		{
			OnTriggerEnter(collider);
		}
	}
}