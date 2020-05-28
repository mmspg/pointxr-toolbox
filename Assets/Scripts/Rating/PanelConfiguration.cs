// Copyright(C) 2020 ECOLE POLYTECHNIQUE FEDERALE DE LAUSANNE, Switzerland
//
//     Multimedia Signal Processing Group(MMSPG)
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program. If not, see<http://www.gnu.org/licenses/>.
//
//
// Created by Nanyang Yang
//
// Reference:
//   E.Alexiou, N. Yang, and T.Ebrahimi, "PointXR: a toolbox for visualization
//   and subjective evaluation of point clouds in virtual reality," 2020 Twelfth
//   International Conference on Quality of Multimedia Experienence (QoMEX)


using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class PanelConfiguration : MonoBehaviour
{
    [HideInInspector]
    public int Size;                // Number of Scores
    [HideInInspector]
    public string[] Description;    // Description of Scores
    [HideInInspector]
    public int[] Scores;            // Scores
    public string Question;         // Question to observers
    [HideInInspector]
    public bool Test;
    public static bool Test_scene;


    void Start()
    {
        Test_scene = Test;

        Text q = GameObject.FindWithTag("Question").GetComponent<Text>();
        Question = Question.Replace("\\n", "\n");
        q.text = Question;
        GameObject button = Resources.Load<GameObject>("RatingButton");
        GameObject panel = GameObject.Find("Panel");

        RectTransform rect = panel.GetComponent<RectTransform>();
        float height = rect.sizeDelta.y;

        if (Size > 1)
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, height + (Size - 1) * 45);

        Size = Scores.Length;
        GameObject[] grades = new GameObject[Size];

        // Assign Scores and Description to buttons 
        for (int i = 0; i < Size; i++)
        {
            grades[i] = Instantiate(button, GameObject.FindWithTag("Score").transform) as GameObject;
            grades[i].name = "" + Scores[i];
            grades[i].GetComponentInChildren<Text>().text = Description[i];
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(PanelConfiguration)), CanEditMultipleObjects]
public class InitiateButtons_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PanelConfiguration script = (PanelConfiguration)target;
        
        // Check if Size is updated
        EditorGUI.BeginChangeCheck();
        script.Size = EditorGUILayout.IntField("Size", script.Size);

        // Update the Size of the array accordingly
        if (EditorGUI.EndChangeCheck())
        {
            script.Scores = new int[script.Size];
            script.Description = new string[script.Size];
            serializedObject.Update(); 
        }

        Show(serializedObject.FindProperty("Description"), false);
        Show(serializedObject.FindProperty("Scores"), false);
        serializedObject.ApplyModifiedProperties();

        script.Test = EditorGUILayout.Toggle("Test scene", script.Test);
    }

    public static void Show(SerializedProperty list, bool show_list_size = true)
    {
        EditorGUILayout.PropertyField(list);
        EditorGUI.indentLevel += 1;
        
            if (show_list_size)
                EditorGUILayout.PropertyField(list.FindPropertyRelative("Array.Size"));
            
            for (int i = 0; i < list.arraySize; i++)
                EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i));
        
        EditorGUI.indentLevel -= 1;
    }
}
#endif
