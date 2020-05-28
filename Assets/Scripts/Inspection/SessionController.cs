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
// Created by Nanyang Yang, contributed by Evangelos Alexiou
//
// Reference:
//   E.Alexiou, N. Yang, and T.Ebrahimi, "PointXR: a toolbox for visualization
//   and subjective evaluation of point clouds in virtual reality," 2020 Twelfth
//   International Conference on Quality of Multimedia Experienence (QoMEX)


using UnityEngine;
using Valve.VR;


public class SessionController : MonoBehaviour {

    private SteamVR_Behaviour_Pose behaviour_pose;
    private SteamVR_Input_Sources input_source;


    void Start () 
    {
        // Initialize the input_source
        behaviour_pose = GetComponent<SteamVR_Behaviour_Pose>();
        input_source = behaviour_pose.inputSource;
    }

    void Update() 
    {
        if (SteamVR_Actions._default.GrabGrip.GetStateDown(input_source))
            InspectionManager.Press_grab = true;
        
        if (SteamVR_Actions._default.GrabPinch.GetStateDown(input_source))
        {
            if (PlayerPrefs.GetString("_Protocol").Contains("Sequential") || PlayerPrefs.GetString("_Protocol").Contains("Alternating"))
                InspectionManager.Press_trigger = true;

            RatingManager.Is_selected = true;
        }
    }
}
