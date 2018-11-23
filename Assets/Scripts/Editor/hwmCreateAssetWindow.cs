using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.IO;

public class hwmCreateAssetWindow : EditorWindow
{
	public Setting m_Setting;

	/// <summary>
	/// 所有程序集名字
	/// </summary>
	private string[] m_AssemblyNames;

	/// <summary>
	/// 所有类名字
	/// </summary>
	private List<string> m_ClassNames;
	private List<string> m_StringCahce;

	[MenuItem("Custom/Utility/Create Asset", false, 0)]
	public static void ShowWindow()
	{
		GetWindow<hwmCreateAssetWindow>("Create Asset", true);
	}

	protected void OnEnable()
	{
		Assembly[] assemblys = AppDomain.CurrentDomain.GetAssemblies();
		m_AssemblyNames = new string[assemblys.Length];
		for (int iAssembly = 0; iAssembly < assemblys.Length; iAssembly++)
		{ 
			m_AssemblyNames[iAssembly] = assemblys[iAssembly].GetName().Name;
		}

		m_ClassNames = new List<string>();
		m_StringCahce = new List<string>();
	}

	protected void OnDisable()
	{
		m_Setting = null;

		m_StringCahce.Clear();
		m_StringCahce = null;

		m_ClassNames.Clear();
		m_ClassNames = null;

		m_AssemblyNames = null;
	}

	protected void OnGUI()
	{
		if (m_Setting == null)
		{
			m_Setting = new Setting();
		}

		PopupItem("程序集", ref m_Setting.AssemblyName, m_AssemblyNames, ref m_Setting.AssemblyNameFilter);
		EditorGUILayout.Space();

		if (string.IsNullOrEmpty(m_Setting.AssemblyName))
		{
			return;
		}

		Assembly assembly = Assembly.Load(m_Setting.AssemblyName);
		if (assembly == null)
		{ 
			return;
		}

		Type[] types = assembly.GetTypes();
		m_ClassNames = new List<string>();
		for (int iType = 0; iType < types.Length; iType++)
		{
			bool isObjectType = string.IsNullOrEmpty(m_Setting.ClassNameBaseTypeFilter);
			Type tp = types[iType];
			while (!isObjectType && tp.BaseType != null)
			{
				tp = tp.BaseType;
				if (tp.FullName == m_Setting.ClassNameBaseTypeFilter)
				{ 
					isObjectType = true;
				}
			}
			if (isObjectType)
			{
				m_ClassNames.Add(types[iType].FullName);
			}
		}

		if (m_ClassNames.Count < 1)
		{ 
			return;
		}

		m_Setting.ClassNameBaseTypeFilter = EditorGUILayout.TextField("父类", m_Setting.ClassNameBaseTypeFilter);

		PopupItem("类", ref m_Setting.ClassName, m_ClassNames.ToArray(), ref m_Setting.ClassNameFilter);
		EditorGUILayout.Space();
		EditorGUILayout.Space();

		if (string.IsNullOrEmpty(m_Setting.ClassName))
		{ 
			return;
		}

		ScriptableObject obj = null;
		try
		{
			obj = CreateInstance(m_Setting.ClassName);
		}
		catch (Exception) 
		{
		}

		if (obj == null)
		{
			return;
		}

		EditorGUILayout.BeginHorizontal();
		m_Setting.AssetDirection = EditorGUILayout.TextField("asset路径", m_Setting.AssetDirection);
		if (GUILayout.Button("选择", GUILayout.Width(120)))
		{
			m_Setting.AssetDirection = EditorUtility.OpenFolderPanel("asset路径", m_Setting.AssetDirection, "");
		}
		EditorGUILayout.EndHorizontal();

		if (string.IsNullOrEmpty(m_Setting.AssetDirection))
		{
			return;
		}
		if (m_Setting.AssetDirection.Contains("Assets"))
		{ 
			m_Setting.AssetDirection = m_Setting.AssetDirection.Substring(m_Setting.AssetDirection.IndexOf("Assets"));
		}
		

		EditorGUILayout.BeginHorizontal();
		m_Setting.AssetFile = EditorGUILayout.TextField("asset文件", m_Setting.AssetFile);
		m_Setting.AssetCover = EditorGUILayout.Toggle("覆盖文件", m_Setting.AssetCover);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		if (!string.IsNullOrEmpty(m_Setting.AssetDirection) 
			&& !string.IsNullOrEmpty(m_Setting.AssetFile)
			&& m_Setting.AssetDirection.Substring(0, 6) == "Assets")
		{
			bool canCreate = m_Setting.AssetCover;
			string assetFile = m_Setting.AssetDirection + "/" + m_Setting.AssetFile + ".asset";
			if (!canCreate)
			{
				canCreate = !File.Exists(Application.dataPath.Remove(Application.dataPath.Length - 6) + assetFile);
			}
			if (canCreate && GUILayout.Button("创建"))
			{
				AssetDatabase.CreateAsset(obj, assetFile);
				Selection.activeObject = obj;
			}
		}
	}

	private void PopupItem(string label, ref string value, string[] strs, ref string filter)
	{
		filter = EditorGUILayout.TextField("筛选", filter);
		if (!string.IsNullOrEmpty(filter))
		{ 
			m_StringCahce.Clear();
			for (int i = 0; i < strs.Length; i++)
			{
				if (strs[i].ToLower().Contains(filter.ToLower()))
				{
					m_StringCahce.Add(strs[i]);
				}
			}
			strs = m_StringCahce.ToArray();
		}

		if (strs.Length < 1)
		{
			value = "";
		}
		else
		{
			int selectIdx = 0;
			if (value == null || value.Length < 1 || Array.IndexOf(strs, value) < 0)
			{
				selectIdx = EditorGUILayout.Popup(label, 0, strs);
			}
			else
			{
				selectIdx = EditorGUILayout.Popup(label, Array.IndexOf<string>(strs, value), strs);
			}

			if (selectIdx >= strs.Length)
			{
				selectIdx = 0;
			}

			value = strs[selectIdx];
		}
	}

	public class Setting
	{
		public string AssemblyName = "Assembly-CSharp";
		public string AssemblyNameFilter;
		public string ClassName;
		public string ClassNameFilter;
		public string ClassNameBaseTypeFilter = "UnityEngine.ScriptableObject";
		public string AssetDirection;
		public string AssetFile;
		public bool AssetCover;
	}
}