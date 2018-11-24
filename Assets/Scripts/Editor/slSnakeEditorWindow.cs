using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class slSnakeEditorWindow : EditorWindow
{
	private const string SNAKE_PREFAB_PATH = "Assets/Resources/Actor/";
	private const string SNAKE_EDITOR_PREfAB_NAME_STARTWITHS = "SnakeEditor_";

	private EditorState m_EditorState;
	private GameObject m_SnakeEditorPrefab;
	private slSnakeEditor m_SnakeEditor;
	private hwmEditorCoroutine m_SnakeEditorUpdateCoroutine;

	[MenuItem("Custom/Snake Loli/Snake Editor", false, 0)]
	public static void ShowWindow()
	{
		GetWindow<slSnakeEditorWindow>("Snake Editor", true);
	}

	protected void OnGUI()
	{
		switch (m_EditorState)
		{
			case EditorState.ChooseSnake:
				EditorGUILayout.HelpBox("Drap snake editor prefab to this", MessageType.Info);
				EditorGUILayout.BeginHorizontal();
				m_SnakeEditorPrefab = EditorGUILayout.ObjectField("Snake Editor Prefab", m_SnakeEditorPrefab, typeof(GameObject), false) as GameObject;
				if (m_SnakeEditorPrefab != null && GUILayout.Button("Editor"))
				{
					m_SnakeEditor = (PrefabUtility.InstantiatePrefab(m_SnakeEditorPrefab) as GameObject).GetComponent<slSnakeEditor>();
					if (m_SnakeEditor != null)
					{
						m_EditorState = EditorState.EditorSnake;
						m_SnakeEditorUpdateCoroutine = hwmEditorCoroutine.Start(UpdateSnakeEditor());
					}
					else
					{
						DestroyImmediate(m_SnakeEditor.gameObject);
						m_SnakeEditor = null;
						EditorUtility.DisplayDialog("Editor snake failed", "Invalid snake editor prefab", "OK");
					}
				}
				EditorGUILayout.EndHorizontal();
				break;
			case EditorState.EditorSnake:
				OnGUI_EditorSnake();
				break;
		}
	}

	protected void OnEnable()
	{
		m_EditorState = EditorState.ChooseSnake;
	}

	protected void OnDisable()
	{
		DisposeEditorSnake();
	}

	private void OnGUI_EditorSnake()
	{
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Back"))
		{
			m_EditorState = EditorState.ChooseSnake;
			DisposeEditorSnake();
		}
		if (GUILayout.Button("Save"))
		{
			OnSaveSnake();
		}
		EditorGUILayout.EndHorizontal();
	}

	private void DisposeEditorSnake()
	{
		if (m_SnakeEditorUpdateCoroutine != null)
		{
			m_SnakeEditorUpdateCoroutine.Stop();
			m_SnakeEditorUpdateCoroutine = null;
		}

		m_SnakeEditorPrefab = null;
		if (m_SnakeEditor != null)
		{
			DestroyImmediate(m_SnakeEditor.gameObject);
			m_SnakeEditor = null;
		}
	}

	private IEnumerator UpdateSnakeEditor()
	{
		while (true)
		{
			int currentOrderInLayer = slConstants.SNAKE_SPRITERENDERER_MAX_ORDERINLAYER;
			if (m_SnakeEditor.Head != null)
			{
				m_SnakeEditor.Head.transform.position = Vector3.zero;
				m_SnakeEditor.Head.transform.rotation = Quaternion.identity;
				ForeachSetSpriteRendererOrderInLayer(m_SnakeEditor.Head, ref currentOrderInLayer);
			}
			if (m_SnakeEditor.Clothes != null)
			{
				m_SnakeEditor.Clothes.transform.position = new Vector3(0, -slConstants.SNAKE_NODE_TO_NODE_DISTANCE, 0);
				m_SnakeEditor.Clothes.transform.rotation = Quaternion.identity;
				ForeachSetSpriteRendererOrderInLayer(m_SnakeEditor.Clothes, ref currentOrderInLayer);
			}
			if (m_SnakeEditor.Body1 != null)
			{
				m_SnakeEditor.Body1.transform.position = new Vector3(0
					, -slConstants.SNAKE_NODE_TO_NODE_DISTANCE * 2
					, 0);
				m_SnakeEditor.Body1.transform.rotation = Quaternion.identity;
				ForeachSetSpriteRendererOrderInLayer(m_SnakeEditor.Body1, ref currentOrderInLayer);
			}
			if (m_SnakeEditor.Body2 != null)
			{
				m_SnakeEditor.Body2.transform.position = new Vector3(0
					, -slConstants.SNAKE_NODE_TO_NODE_DISTANCE * 3
					, 0);
				m_SnakeEditor.Body2.transform.rotation = Quaternion.identity;
				ForeachSetSpriteRendererOrderInLayer(m_SnakeEditor.Body2, ref currentOrderInLayer);
			}
			if (m_SnakeEditor.Body3 != null)
			{
				m_SnakeEditor.Body3.transform.position = new Vector3(0
					, -slConstants.SNAKE_NODE_TO_NODE_DISTANCE * 4
					, 0);
				m_SnakeEditor.Body3.transform.rotation = Quaternion.identity;
				ForeachSetSpriteRendererOrderInLayer(m_SnakeEditor.Body3, ref currentOrderInLayer);
			}
			yield return null;
		}
	}

	private void ForeachSetSpriteRendererOrderInLayer(GameObject root, ref int currentOrderInLayer)
	{
		SpriteRenderer[] sprites = root.GetComponentsInChildren<SpriteRenderer>();
		for (int iSprite = 0; iSprite < sprites.Length; iSprite++)
		{
			SpriteRenderer iterSprite = sprites[iSprite];
			iterSprite.sortingOrder = currentOrderInLayer--;
			iterSprite.sortingLayerID = slConstants.SORTINGLAYER_SNAKE;
		}
	}

	private void OnSaveSnake()
	{
		PrefabUtility.ReplacePrefab(m_SnakeEditor.gameObject, PrefabUtility.GetPrefabParent(m_SnakeEditor.gameObject), ReplacePrefabOptions.ConnectToPrefab);
		string snakeName = m_SnakeEditor.gameObject.name.Remove(0, SNAKE_EDITOR_PREfAB_NAME_STARTWITHS.Length);

		string snakePrefabPath = string.Format("{0}{1}{2}.prefab", SNAKE_PREFAB_PATH, slConstants.SNAKE_PREfAB_NAME_STARTWITHS, snakeName);
		GameObject snakeGameObject = new GameObject(slConstants.SNAKE_PREfAB_NAME_STARTWITHS + snakeName);
		slSnake snake = snakeGameObject.AddComponent<slSnake>();
		snake.MyProperties = new slSnake.Properties();
		snake.MyProperties.SnakeName = snakeName;
		snake.MyProperties.HeadColliderRadius = m_SnakeEditor.Head.GetComponent<CircleCollider2D>().radius;
		snake.MyProperties.ClothesColliderRadius = m_SnakeEditor.Clothes.GetComponent<CircleCollider2D>().radius;
		snake.MyProperties.BodyColliderRadius = m_SnakeEditor.Body1.GetComponent<CircleCollider2D>().radius;
		snake.MyProperties.DeadFoodColor = m_SnakeEditor.DeadFoodColor;
		PrefabUtility.CreatePrefab(snakePrefabPath, snakeGameObject);
		DestroyImmediate(snakeGameObject);

		string snakePresentationPrefabPath = string.Format("{0}{1}{2}.prefab", SNAKE_PREFAB_PATH, slConstants.SNAKE_PRESENTATION_PREfAB_NAME_STARTWITHS, snakeName);
		GameObject snakePresentationGameObject = new GameObject(slConstants.SNAKE_PRESENTATION_PREfAB_NAME_STARTWITHS + snakeName);
		slSnakePresentation snakePresentation = snakePresentationGameObject.AddComponent<slSnakePresentation>();
		GameObject snakePresentationPropertiesGameObject = new GameObject("Properties");
		snakePresentationPropertiesGameObject.SetActive(false);
		snakePresentationPropertiesGameObject.transform.SetParent(snakePresentationGameObject.transform);
		snakePresentationPropertiesGameObject.transform.position = new Vector3(10000, 10000, 0);
		snakePresentation.MyProperties = new slSnakePresentation.Properties();
		snakePresentation.MyProperties.Head = InstantiateFromSnakeEditorToSnakePresentation(m_SnakeEditor.Head, snakePresentationPropertiesGameObject);
		snakePresentation.MyProperties.Clothes = InstantiateFromSnakeEditorToSnakePresentation(m_SnakeEditor.Clothes, snakePresentationPropertiesGameObject);
		snakePresentation.MyProperties.Body = InstantiateFromSnakeEditorToSnakePresentation(m_SnakeEditor.Body1, snakePresentationPropertiesGameObject);
		snakePresentation.MyProperties.BodySpriteMaxOrderInLayer = snakePresentation.MyProperties.Body.GetComponent<SpriteRenderer>().sortingOrder;
		PrefabUtility.CreatePrefab(snakePresentationPrefabPath, snakePresentationGameObject);
		DestroyImmediate(snakePresentationGameObject);
	}

	private GameObject InstantiateFromSnakeEditorToSnakePresentation(GameObject snakeEditor, GameObject snakePresentation)
	{
		GameObject go = Instantiate(snakeEditor);
		DestroyImmediate(go.GetComponent<CircleCollider2D>());
		go.name = go.name.Replace("(Clone)", "");
		go.transform.SetParent(snakePresentation.transform);
		go.transform.transform.localPosition = Vector3.zero;
		return go;
	}

	private enum EditorState
	{
		ChooseSnake,
		EditorSnake
	}
}