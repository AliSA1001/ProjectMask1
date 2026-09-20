using UnityEditor;
using UnityEngine;

// Play / Skip buttons under the Cutscene component so you can test a cutscene without setting up a trigger
[CustomEditor(typeof(Cutscene))]
public class CutsceneInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Play", GUILayout.Height(28f))) ((Cutscene)target).Play();
            if (GUILayout.Button("Skip", GUILayout.Height(28f))) ((Cutscene)target).Skip();
            EditorGUILayout.EndHorizontal();
        }
        if (!Application.isPlaying)
            EditorGUILayout.HelpBox("Press Play in Unity to test it here. To edit the shots, open Window > Sequencing > Timeline.", MessageType.Info);
    }
}
