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
//
// This script handles the functionality required to display the panel and store
// the score of a user from the Rating scene. 
// The questions and answers can be specified in the Editor.
//
//
// Reference:
//   E.Alexiou, N. Yang, and T.Ebrahimi, "PointXR: a toolbox for visualization
//   and subjective evaluation of point clouds in virtual reality," 2020 Twelfth
//   International Conference on Quality of Multimedia Experienence (QoMEX)


using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public class RatingManager : BaseInputModule
{
    public static bool Is_selected;
    private PointerEventData pointer = null;
    private GameObject button;


    protected override void Awake()
    {
        base.Awake();
        pointer = new PointerEventData(eventSystem);
        Is_selected = false;
        button = null;
    }

    public override void Process()
    {
        pointer.Reset();
        
        Camera cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        pointer.position = new Vector2(cam.pixelWidth / 2, cam.pixelHeight / 2);

        // Raycast
        eventSystem.RaycastAll(pointer, m_RaycastResultCache);
        pointer.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
        button = pointer.pointerCurrentRaycast.gameObject;
  
        // Clear cache 
        m_RaycastResultCache.Clear();

        // Hover handler
        HandlePointerExitAndEnter(pointer, button);
        
        // If selected
        if (Is_selected)
        {
            Is_selected = false;
            OnPressTrigger(pointer);
        }
    }
    
    private void OnPressTrigger(PointerEventData data)
    {
        data.pointerPressRaycast = data.pointerCurrentRaycast;
        GameObject choice = ExecuteEvents.ExecuteHierarchy(button, data, ExecuteEvents.pointerDownHandler);
        choice = ExecuteEvents.GetEventHandler<IPointerClickHandler>(button);

        // Get the is_selected score
        eventSystem.SetSelectedGameObject(choice);
        if (choice != null)
        {
            // Store subjective score
            if (InspectionManager.Store_scores)
            {
                LogScores.StoreData(choice.name);
                PlayerPrefs.SetInt("_Save_score", 1);
            }
            
            // Reset
            eventSystem.SetSelectedGameObject(null);
            data.position = Vector2.zero;
            data.pointerPress = null;
            data.rawPointerPress = null;

            // Switch back to the 'Inspection' scene
            if (!PanelConfiguration.Test_scene)
                GoToInspectionScene();
            else
            {
                GameObject.FindGameObjectWithTag("Panel").SetActive(false);
                Debug.Log("Selected score: " + choice.name);
            }
        }
    }
    
    private void GoToInspectionScene()
    {
        SceneManager.LoadScene(sceneName: "Inspection");
    }
}
