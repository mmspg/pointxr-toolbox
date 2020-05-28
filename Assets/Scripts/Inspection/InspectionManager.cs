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
// Created by Peisen Xu, modified by Nanyang Yang, contributed by Evangelos Alexiou
//
//
// This script handles the functionality required to host the subjective evaluation
// protocol and, potentially, the corresponding variant that is selected by the user.
// Additional options, such as enabling or disabling the model rating and the recording
// of the user's interactions are provided. 
// As input, a batch file is required to specify the models and the order of presentation
// that is followed during an evaluation session.
// The aforementioned options can be selected in the Editor.
// The Inspection scene can be also used as a simple viewer for both point clouds and 
// prefabs, when used independently of the Rating scene (disable "log user scores").
// 
//
// Reference:
//   E.Alexiou, N. Yang, and T.Ebrahimi, "PointXR: a toolbox for visualization
//   and subjective evaluation of point clouds in virtual reality," 2020 Twelfth
//   International Conference on Quality of Multimedia Experienence (QoMEX)


using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;

// customise the editor interface under inspector
#if UNITY_EDITOR
using UnityEditor;
#endif


public class InspectionManager : MonoBehaviour
{
    // Inspector IO variables
    [HideInInspector]
    public string Batch_file;
    [HideInInspector]
    public EvaluationProtocol Protocol;
    [HideInInspector]
    public ProtocolVariation Variation;
    [HideInInspector]
    public bool Log_interaction;
    [HideInInspector]
    public bool Log_scores;

    // User defined paramteres
    private readonly float show_delay = 0.25f;

    // Operational variables and flag
    public static Storyboard Session;
    public static bool Start_session = true;
    public static bool Press_grab;
    public static bool Press_trigger;
    private static bool Allow_toggling;
    public static bool Has_toggled;
    public static bool Is_reference;
    public static bool Store_interaction;
    public static bool Store_scores;

    // Paths
    private readonly string path_to_batches = "Assets/Resources/Batches/";
    public static string path_to_logs = "Assets/Resources/Logs/";

    public enum EvaluationProtocol {Single_stimulus, Double_stimulus}

    public enum ProtocolVariation {Sequential, Simultaneous, Alternating}

    [System.Serializable]
    public class Step
    {
        public string Reference;
        public string Distorted;
        public bool Stage;
    }

    [System.Serializable]
    public class Storyboard
    {
        public Step[] Steps;
    }
    

    void Awake()
    {
        Press_grab = false;
        Press_trigger = false;
        Is_reference = true;
        Has_toggled = false;

        LoadObjects.Model_reference = null;
        LoadObjects.Model_distorted = null;

        // Pass filename to Log script for output file generation
        if (Start_session)
        {
            // Set session
            SetSession();

            // Initialize
            Store_interaction = Log_interaction;
            Store_scores = Log_scores;

            PlayerPrefs.SetInt("_StartSession", 1);
            PlayerPrefs.SetString("_Batch", Batch_file);
            if (Protocol.Equals(EvaluationProtocol.Single_stimulus))
                PlayerPrefs.SetString("_Protocol", Protocol.ToString());
            else
                PlayerPrefs.SetString("_Protocol", Protocol.ToString() + "-" + Variation.ToString());
            PlayerPrefs.SetInt("_Step", 0);
                        
            Allow_toggling = PlayerPrefs.GetString("_Protocol").Contains("Alternating") || PlayerPrefs.GetString("_Protocol").Contains("Sequential");

            LoadObjects.GetNext();

            Start_session = false;
        }
       
        // Render at a platform's default frame rate, specified by the SDK and ignore values specified by the game
        Application.targetFrameRate = -1;
    }
    
    private void SetSession()
    {
        if (Batch_file == null)
        {
            PlayerPrefs.DeleteAll();
            Debug.LogError("Empty batch filename");
            EditorApplication.ExecuteMenuItem("Edit/Play");
        }

        Batch_file = Path.GetFileNameWithoutExtension(Batch_file);
        string file_in = path_to_batches + Batch_file + ".json";
        if (File.Exists(file_in))
        {
            Session = JsonUtility.FromJson<Storyboard>(File.ReadAllText(file_in));
        }
        else
        {
            Debug.LogError("Batch not found");
            EditorApplication.ExecuteMenuItem("Edit/Play");
        }
    }

    void Update()
    {
        // If press grab in Inspection scene
        if (Press_grab)
            OnPressGrab();
        
        // If press trigger in Inspection scene
        if (Press_trigger)
            OnPressTrigger();

        // If return from Rating scene
        if (Log_scores && PlayerPrefs.GetInt("_Save_score") == 1)
        {
            PlayerPrefs.SetInt("_Save_score", 0);
            if (PlayerPrefs.GetInt("_Step") < Session.Steps.Length)
                LoadObjects.GetNext();
            else
                FinishSession();
        }
    }

    private void OnPressGrab()
    {
        Press_grab = false;
        if (!Allow_toggling || (Allow_toggling && Has_toggled))
        {
            PlayerPrefs.SetInt("_Step", PlayerPrefs.GetInt("_Step") + 1);
            PlayerPrefs.SetString("_EndTime", System.DateTime.Now.ToString("HHmmssfff"));
            if (Log_scores)
            {
                GoToRatingScene();
            }
            else
            {
                if (PlayerPrefs.GetInt("_Step") < Session.Steps.Length)
                {
                    Is_reference = true;
                    Has_toggled = false;
                    if (Log_interaction)
                        LogInteractions.CloseFile();
                    LoadObjects.GetNext();
                    if (Log_interaction)
                        LogInteractions.OpenFile();
                }
                else
                {
                    FinishSession();
                }
            }
        }
    }

    private void OnPressTrigger()
    {
        Press_trigger = false;
        if ((PlayerPrefs.GetString("_Protocol").Contains("Alternating")) ||
            (!Has_toggled && PlayerPrefs.GetString("_Protocol").Contains("Sequential") && Is_reference))
        {
            Has_toggled = true;
            Toggle();
        }  
    }

    private void Toggle()
    {
        if (Is_reference)
        {
            Is_reference = false;
            LoadObjects.Model_reference.SetActive(false);
            LoadObjects.Sign_reference.SetActive(false);
            StartCoroutine(LateCall());
        }
        else
        {
            Is_reference = true;
            LoadObjects.Model_distorted.SetActive(false);
            LoadObjects.Sign_distorted.SetActive(false);
            StartCoroutine(LateCall());
        }
    }

    IEnumerator LateCall()
    {
        yield return new WaitForSeconds(show_delay);

        if (!Is_reference)
        {
            LoadObjects.Model_distorted.SetActive(true);
            LoadObjects.Sign_distorted.SetActive(true);
        }
        else
        {
            LoadObjects.Model_reference.SetActive(true);
            LoadObjects.Sign_reference.SetActive(true);
        }
    }

    private void FinishSession()
    {
        if(LoadObjects.Model_reference !=null )
            LoadObjects.Model_reference.SetActive(false);
        if (LoadObjects.Model_distorted != null)
            LoadObjects.Model_distorted.SetActive(false);
        if (LoadObjects.Sign_reference != null)
            LoadObjects.Sign_reference.SetActive(false);
        if (LoadObjects.Sign_distorted != null)
            LoadObjects.Sign_distorted.SetActive(false);
        if (LoadObjects.Stage != null)
            LoadObjects.Stage.SetActive(false);
        if (LoadObjects.Stage_reference != null)
            LoadObjects.Stage_reference.SetActive(false);
        if (LoadObjects.Stage_distorted != null)
            LoadObjects.Stage_distorted.SetActive(false);
        if (gameObject != null)
            gameObject.SetActive(false);
        PlayerPrefs.DeleteAll();
        //EditorApplication.ExecuteMenuItem("Edit/Play");
    }

    private void GoToRatingScene()
    {
        SceneManager.LoadScene(sceneName: "Rating");
    }
}


// customize inspectorGUI
#if UNITY_EDITOR
[CustomEditor(typeof(InspectionManager)), CanEditMultipleObjects]
public class InpsectionManager_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // for other non-HideInInspector fields

        InspectionManager script = (InspectionManager)target;

        script.Batch_file = EditorGUILayout.TextField("Batch", script.Batch_file);
        script.Protocol = (InspectionManager.EvaluationProtocol)EditorGUILayout.EnumPopup("Protocol", script.Protocol);
        if (script.Protocol == InspectionManager.EvaluationProtocol.Double_stimulus) // if bool is true, show other fields
        {
            script.Variation = (InspectionManager.ProtocolVariation)EditorGUILayout.EnumPopup("Variation", script.Variation);
        }
        script.Log_scores = EditorGUILayout.Toggle("Log user scores", script.Log_scores);
        script.Log_interaction = EditorGUILayout.Toggle("Log user interaction", script.Log_interaction);
    }
}
#endif
