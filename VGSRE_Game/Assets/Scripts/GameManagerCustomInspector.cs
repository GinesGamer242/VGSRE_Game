using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
public class GameManagerCustomInspector : Editor
{
    static void Init()
    {
        EditorWindow window = GetWindow(typeof(EditorGUILayoutTagField));
        window.Show();
    }

    public override void OnInspectorGUI()
    {
        GameManager myTarget = (GameManager)target;

        myTarget.m_ObjectTag = EditorGUILayout.TagField();

        for (int i = 0; i < myTarget.m_RoundAmount; ++i)
        {

        }
    }
}
