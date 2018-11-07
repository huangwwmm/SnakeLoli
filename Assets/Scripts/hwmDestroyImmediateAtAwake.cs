using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hwmDestroyImmediateAtAwake : MonoBehaviour
{
	protected void Awake()
	{
		DestroyImmediate(gameObject);
	}
}