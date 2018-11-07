using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Linq;

public class snAssetRelationsInProjectWindow : EditorWindow
{
	/// <summary>
	/// 保存在 ProjectRoot/RELATIONS_RECORD_FILE_PATH
	/// </summary>
	private const string RELATIONS_RECORD_FILE_PATH = "Relations.bin";
	/// <summary>
	/// 每帧加载的数量
	/// 数值太高会造成Unity假死 太低会增加加载时间
	/// </summary>
	private const int FRAME_LOAD_COUNT = 50;

	private RelationGraph m_Graph;
	private bool m_Loading = false;
	/// <summary>
	/// 加载进度 0~1
	/// 获取所有文件、保存到文件的时间不足1秒 忽略不计
	/// </summary>
	private float m_LoadingProgress = 0;

	/// <summary>
	/// 当前的Asset
	/// </summary>
	private UnityEngine.Object m_TargetObject;
	/// <summary>
	/// 上一帧的Asset
	/// </summary>
	private UnityEngine.Object m_LastTargetObject;

	private UnityEngine.Object[] m_ReferencesBuffer;
	private UnityEngine.Object[] m_DependenciesBuffer;

	private Vector2 m_ReferenceScrollPosition = Vector2.zero;
	private Vector2 m_DependencieScrollPosition = Vector2.zero;
	private string m_RecordFilePath;

	[MenuItem("Custom/Utility/Asset Relations In Project")]
	public static void ShowWindow()
	{
		snAssetRelationsInProjectWindow window = GetWindow(typeof(snAssetRelationsInProjectWindow)) as snAssetRelationsInProjectWindow;
		window.titleContent = new GUIContent("Relations");
	}

	protected void OnEnable()
	{
		m_RecordFilePath = string.Format("{0}/{1}", Directory.GetCurrentDirectory(), RELATIONS_RECORD_FILE_PATH);
		LoadAllRelationFromFile(false); // Q：为什么这里不输出LOG A：修改代码后返回UnityEditor都会触发这里，弹Log的Dialog太烦了
	}

	protected void Destroy()
	{
		m_ReferencesBuffer = null;
		m_DependenciesBuffer = null;
	}

	protected void OnGUI()
	{
		if (m_Loading)
		{
			EditorGUILayout.HelpBox(String.Format("加载进度: {0:F2}% (鼠标点击当前窗口来刷新加载进度)\n加载过程中不要操作当前的Unity"
				, m_LoadingProgress * 100.0f), MessageType.Info);
			return;
		}

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("从缓存文件加载所有Asset的引用关系"))
		{
			LoadAllRelationFromFile();
		}
		if (GUILayout.Button("重新加载所有Asset的引用关系并保存到缓存文件"))
		{
			if (EditorUtility.DisplayDialog("提示", "确定要执行这个操作？这个操作可能需要数分钟", "确认", "取消"))
			{
				EditorCoroutine.Start(LoadAllRelationsAndSaveRecordFile());
			}
		}
		EditorGUILayout.EndHorizontal();

		if (m_Graph != null)
		{
			EditorGUILayout.Space();

			m_LastTargetObject = m_TargetObject;
			m_TargetObject = EditorGUILayout.ObjectField("Target", m_TargetObject, typeof(UnityEngine.Object), false);

			if (LoadAssetRelation())
			{
				EditorGUILayout.Space();
				GUILayout.Label("References Count：" + m_ReferencesBuffer.Length);
				m_ReferenceScrollPosition = EditorGUILayout.BeginScrollView(m_ReferenceScrollPosition);
				for (int iReference = 0; iReference < m_ReferencesBuffer.Length; iReference++)
				{
					EditorGUILayout.ObjectField(m_ReferencesBuffer[iReference], typeof(UnityEngine.Object), false);
				}
				EditorGUILayout.EndScrollView();

				EditorGUILayout.Space();
				GUILayout.Label("Dependencies Count：" + m_DependenciesBuffer.Length);
				m_DependencieScrollPosition = EditorGUILayout.BeginScrollView(m_DependencieScrollPosition);
				for (int iDependencie = 0; iDependencie < m_DependenciesBuffer.Length; iDependencie++)
				{
					EditorGUILayout.ObjectField(m_DependenciesBuffer[iDependencie], typeof(UnityEngine.Object), false);
				}
				EditorGUILayout.EndScrollView();
			}
		}
		else
		{
			EditorGUILayout.HelpBox("还未获取引用关系", MessageType.Warning);
		}
	}

	private bool LoadAssetRelation()
	{
		if (m_TargetObject != null)
		{
			if (m_LastTargetObject != m_TargetObject)
			{
				string assetPath = AssetDatabase.GetAssetPath(m_TargetObject);
				if (string.IsNullOrEmpty(assetPath))
				{
					EditorGUILayout.HelpBox("这个Asset不存在，是不是修改了项目文件？重新加载引用关系可以解决", MessageType.Warning);
				}
				else if (!m_Graph.Nodes.ContainsKey(assetPath))
				{
					EditorGUILayout.HelpBox("未找到这个Asset的引用。请尝试重新加载引用关系", MessageType.Warning);
				}
				else
				{
					RelationGraph.Node node = m_Graph.Nodes[assetPath];

					m_ReferencesBuffer = LoadAssets(node.References);
					m_DependenciesBuffer = LoadAssets(node.Dependencies);
				}
			}
		}
		else
		{
			EditorGUILayout.HelpBox("拖拽Asset至上方\"Target\" Field后，会显示Asset的引用关系", MessageType.Info);
			m_ReferencesBuffer = null;
			m_DependenciesBuffer = null;
		}

		return m_ReferencesBuffer != null && m_DependenciesBuffer != null;
	}

	private UnityEngine.Object[] LoadAssets(string[] assetPaths)
	{
		List<UnityEngine.Object> objects = new List<UnityEngine.Object>();

		for (int iAsset = 0; iAsset < assetPaths.Length; iAsset++)
		{
			UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(assetPaths[iAsset], typeof(UnityEngine.Object));
			if (obj == null)
			{
				continue;
			}
			objects.Add(obj);
		}
		return objects.ToArray();
	}

	/// <summary>
	/// 从文件中加载引用关系
	/// </summary>
	private bool LoadAllRelationFromFile(bool enableLog = true)
	{
		try
		{
			if (File.Exists(m_RecordFilePath))
			{
				m_Graph = (RelationGraph)SerializeUtility.ReadFile(m_RecordFilePath);
				if (enableLog)
				{
					EditorUtility.DisplayDialog("提示", "读取引用关系成功.", "确定");
				}
				return true;
			}
			else
			{
				if (enableLog)
				{
					EditorUtility.DisplayDialog("提示", "Asset引用关系缓存文件不存在，您需要先加载所有所有Asset的引用关系.", "确定");
				}
				return false;
			}

		}
		catch (Exception ex)
		{
			if (enableLog)
			{
				EditorUtility.DisplayDialog("提示", "读取引用关系失败.\n" + ex.ToString(), "确定");
			}
			return false;
		}
	}

	/// <summary>
	/// 加载项目里所有文件的引用关系
	/// </summary>
	/// <param name="loadAssetCount">加载的Asset数量，小于0时，加载所有Asset。载所有Asset时间很长，为了方便测试，加这个参数</param>
	private IEnumerator LoadAllRelationsAndSaveRecordFile(int loadAssetCount = -1)
	{
		m_Loading = true;
		m_LoadingProgress = 0;
		if (m_Graph == null)
		{
			m_Graph = new RelationGraph();
		}
		m_Graph.Nodes.Clear();
		string[] guids = AssetDatabase.FindAssets("");

		for (int guidIdx = 0; guidIdx < (loadAssetCount < 0 ? guids.Length : loadAssetCount); guidIdx++)
		{
			RelationGraph.Node node = new RelationGraph.Node();
			node.AssetPath = AssetDatabase.GUIDToAssetPath(guids[guidIdx]);
			if (!m_Graph.Nodes.ContainsKey(node.AssetPath))
			{
				m_Graph.Nodes.Add(node.AssetPath, node);
			}
		}
		yield return null;

		int allCount = m_Graph.Nodes.Count;
		int loadedCount = 0;
		foreach (KeyValuePair<string, RelationGraph.Node> kv in m_Graph.Nodes)
		{
			RelationGraph.Node node = kv.Value;
			node.Dependencies = AssetDatabase.GetDependencies(node.AssetPath, false);
			for (int dependencieIdx = 0; dependencieIdx < node.Dependencies.Length; dependencieIdx++)
			{
				string dependencie = node.Dependencies[dependencieIdx];
				if (m_Graph.Nodes.ContainsKey(dependencie))
				{
					m_Graph.Nodes[dependencie].ReferencesCache.Add(node.AssetPath);
				}
			}

			if (++loadedCount % FRAME_LOAD_COUNT == 0)
			{
				m_LoadingProgress = (float)loadedCount / (float)allCount;
				yield return null;
				Focus();
			}
		}
		yield return null;

		// 将ReferencesCache转换到References
		foreach (KeyValuePair<string, RelationGraph.Node> kv in m_Graph.Nodes)
		{
			RelationGraph.Node node = kv.Value;

			node.References = node.ReferencesCache.ToArray();
		}

		try
		{
			SerializeUtility.WriteFile(m_RecordFilePath, m_Graph);
			EditorUtility.DisplayDialog("提示", "加载引用关系并保存到文件成功", "确定");
		}
		catch (Exception ex)
		{
			EditorUtility.DisplayDialog("提示", "加载引用关系并保存到文件失败\n" + ex.ToString(), "确定");
		}
		finally
		{
			m_Loading = false;
		}
	}

	[Serializable]
	/// <summary>
	/// 关系图
	/// </summary>
	public class RelationGraph
	{
		public Dictionary<string, Node> Nodes = new Dictionary<string, Node>();

		[Serializable]
		public class Node
		{
			public string AssetPath;

			public string[] Dependencies;
			public string[] References;

			/// <summary>
			/// 从文件加载的RelationGraph时，这个变量为空
			/// </summary>
			[NonSerialized]
			public HashSet<string> ReferencesCache = new HashSet<string>();
		}
	}

	/// <summary>
	/// 通用方法写在这里是为了方便工具从项目中分离
	/// </summary>
	public class EditorCoroutine
	{
		private readonly IEnumerator m_Routine;
		EditorCoroutine(IEnumerator routine)
		{
			m_Routine = routine;
		}

		public static EditorCoroutine Start(IEnumerator routine)
		{
			EditorCoroutine coroutine = new EditorCoroutine(routine);
			coroutine.Start();
			return coroutine;
		}

		private void Start()
		{
			EditorApplication.update += Update;
		}

		public void Stop()
		{
			EditorApplication.update -= Update;
		}

		private void Update()
		{
			if (!m_Routine.MoveNext())
			{
				Stop();
			}
		}
	}

	/// <summary>
	/// 通用方法写在这里是为了方便工具从项目中分离
	/// </summary>
	public class SerializeUtility
	{
		/// <summary>
		/// 写入文件 需要处理异常
		/// </summary>
		/// <param name="fileFullName">文件完整路径</param>
		/// <param name="graph">需要保存的数据</param>
		public static void WriteFile(string fileFullName, object graph)
		{
			FileStream fs = null;
			try
			{
				if (File.Exists(fileFullName))
				{
					File.Delete(fileFullName);
				}
				fs = new FileStream(fileFullName, FileMode.OpenOrCreate, FileAccess.Write);
				BinaryFormatter bf = new BinaryFormatter();
				bf.Serialize(fs, graph);
			}
			catch (Exception ex)
			{
				throw (ex);
			}
			finally
			{
				if (fs != null)
				{
					fs.Close();
				}
			}
		}

		/// <summary>
		/// 读取文件 需要处理异常
		/// </summary>
		/// <param name="fileFullName">文件完整路径</param>
		/// <returns>读取到的数据</returns>
		public static object ReadFile(string fileFullName)
		{
			FileStream fs = null;
			try
			{
				fs = new FileStream(fileFullName, FileMode.OpenOrCreate);
				BinaryFormatter bf = new BinaryFormatter();
				return bf.Deserialize(fs);
			}
			catch (Exception ex)
			{
				throw (ex);
			}
			finally
			{
				if (fs != null)
				{
					fs.Close();
				}
			}
		}
	}
}