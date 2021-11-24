using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class PlacementToolsWindow : EditorWindow
{
	#region Properties
	//-----------------------------------------------------------------
	//public bool MyProperty {get; set;}
	//public bool MyProperty { get { return x; } set { x = value; } }
	//-----------------------------------------------------------------
	#endregion Properties

	#region Members
	//-----------------------------------------------------------------

	[SerializeField]
	private Transform m_OriginTransform;
	[SerializeField]
	private float m_MaxColumnCount = 10;
	[SerializeField]
	private float m_OffsetColumn = 1;
	[SerializeField]
	private float m_OffsetRow = 1;
	[SerializeField]
	private int m_SingleSpawnCount = 1;
	[SerializeField]
	private Vector3 m_InitialRotation = Vector3.zero;

	//-----------------------------------------------------------------
	#endregion Members

	#region Unity Overloads
	//-----------------------------------------------------------------

	[MenuItem("Tools/Placement Tools...", false, 0)]
	private static void Init()
	{
		//create new window
		EditorWindow.GetWindow<PlacementToolsWindow>(false, "Placement Tools", true);
	}

	private void OnGUI()
	{
		if (!this)
			return;

		this.titleContent = new GUIContent("Place Tools", EditorGUIUtility.FindTexture("ToolHandleLocal"));

		EditorGUILayout.BeginVertical();
		{
			m_OriginTransform = EditorGUILayout.ObjectField("Placement Origin", m_OriginTransform, typeof(Transform), true) as Transform;

			m_MaxColumnCount = EditorGUILayout.FloatField("Max Column Count", m_MaxColumnCount);
			m_OffsetColumn = EditorGUILayout.FloatField("Column Offset", m_OffsetColumn);
			m_OffsetRow = EditorGUILayout.FloatField("Row Offset", m_OffsetRow);
			m_SingleSpawnCount = EditorGUILayout.IntField("Spawn Count", m_SingleSpawnCount);
			m_InitialRotation = EditorGUILayout.Vector3Field("Initial Rotation", m_InitialRotation);

			List<Object> selectedPrefabsList = new List<Object>();

			{
				Object[] selectedAssetsArr = Selection.GetFiltered(typeof(Object), SelectionMode.Assets | SelectionMode.TopLevel);
				
				foreach (Object obj in selectedAssetsArr)
				{
					PrefabAssetType prefabAssetType = PrefabUtility.GetPrefabAssetType(obj);

					if (prefabAssetType != PrefabAssetType.NotAPrefab)
					{
						selectedPrefabsList.Add(obj);
					}
					else
					{
						//Debug.LogWarning("Object: " + obj.name + " is not a valid prefab. Type: " + prefabType.ToString());
					}
				}
			}

			EditorGUILayout.LabelField("Selected Prefabs: " + selectedPrefabsList.Count, GUILayout.ExpandWidth(true), GUILayout.MinWidth(0));

			if (selectedPrefabsList.Count <= 0)
			{
				GUI.enabled = false;
			}

			if (GUILayout.Button("Instantiate"))
			{
				InstantiatePrefabs(selectedPrefabsList);
			}

			if (selectedPrefabsList.Count <= 0)
			{
				GUI.enabled = true;
			}
		}
		EditorGUILayout.EndVertical();
	}

	//-----------------------------------------------------------------
	#endregion Unity Overloads

	#region Public Interface
	//-----------------------------------------------------------------
	//public void MyFunction()
	//{
	//
	//}
	//-----------------------------------------------------------------
	#endregion Public Interface

	#region Private Interface
	//-----------------------------------------------------------------

	private void InstantiatePrefabs(IEnumerable<Object> selectedPrefabsList)
	{
		List<GameObject> selectedPrefabGameObjectsList = selectedPrefabsList.Select(x => EditorUtility.InstanceIDToObject(x.GetInstanceID()) as GameObject).Where(x => (x != null)).OrderBy(x => x.name).ToList();

		int currentColumn = 0;
		int currentRow = 0;

		foreach (var selectedPrefabGameObject in selectedPrefabGameObjectsList)
		{
			for (int i = 0; i < m_SingleSpawnCount; i++)
			{
				Vector3 originPosition = m_OriginTransform.position;
				{
					originPosition.x += (currentColumn * m_OffsetColumn);
					originPosition.z += (currentRow * m_OffsetRow);
				}
				
				GameObject instantiatedGameObject = PrefabUtility.InstantiatePrefab(selectedPrefabGameObject) as GameObject;
				
				if (instantiatedGameObject != null)
				{
					Undo.RegisterCreatedObjectUndo(instantiatedGameObject, "Place Object");

					instantiatedGameObject.transform.position = originPosition;
					instantiatedGameObject.transform.rotation = Quaternion.Euler(m_InitialRotation);

					//Vector3 originalScale = go.transform.localScale;
					instantiatedGameObject.transform.SetParent(m_OriginTransform);
					//go.transform.localScale = originalScale;
				}

				++currentColumn;

				if (currentColumn > m_MaxColumnCount)
				{
					currentColumn = 0;
					++currentRow;
				}
			}
		}
	}

	//-----------------------------------------------------------------
	#endregion Private Interface
}
