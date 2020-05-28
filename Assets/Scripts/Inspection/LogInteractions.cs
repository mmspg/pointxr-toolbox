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
// Reference:
//   E.Alexiou, N. Yang, and T.Ebrahimi, "PointXR: a toolbox for visualization
//   and subjective evaluation of point clouds in virtual reality," 2020 Twelfth
//   International Conference on Quality of Multimedia Experienence (QoMEX)


using UnityEngine;
using System.IO;
using System.Collections;


public class LogInteractions : MonoBehaviour
{
    private static StreamWriter s_writer;
    private static Camera cam;
    private readonly float buffer = 0.1f;


    void Awake()
    {
        if (InspectionManager.Store_interaction)
        {
            cam = Camera.main;
            OpenFile();
        }
    }

    void Update()
    {
        // If streamwriter is initialised, start recording the cam position information
        if (InspectionManager.Store_interaction && s_writer != null) 
            StartCoroutine(StoreData(Time.realtimeSinceStartup, cam.transform.position, cam.transform.eulerAngles)); 
    }

    public static void OpenFile()
    {
        if (PlayerPrefs.GetInt("_Step") < InspectionManager.Session.Steps.Length)
        {
            DirectoryInfo dir;
            dir = Directory.CreateDirectory(InspectionManager.path_to_logs + PlayerPrefs.GetString("_Batch"));

            string file_name = PlayerPrefs.GetInt("_Step").ToString("0000") + "_" + InspectionManager.Session.Steps[PlayerPrefs.GetInt("_Step")].Distorted + "_" + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            s_writer = new StreamWriter(dir.FullName + "/" + file_name + ".csv", true);
            s_writer.WriteLine("TimeStamp,CamPos.x,CamPos.y,CamPos.z,CamRot.x,CamRot.y,CamRot.z,Is_reference,Time");
            s_writer.Flush();
        }
    }

    IEnumerator StoreData(float timestamp, Vector3 camPos, Vector3 camRot)
    {
        yield return new WaitForSeconds(buffer);
        s_writer.WriteLine(timestamp.ToString() + "," + camPos.x.ToString() + "," + camPos.y.ToString() + "," + camPos.z.ToString() + "," +
                camRot.x.ToString() + "," + camRot.y.ToString() + "," + camRot.z.ToString() + "," + InspectionManager.Is_reference.ToString() + "," + System.DateTime.Now.ToString("HHmmssfff"));
        s_writer.Flush();
    }

    public static void CloseFile()
    {
        if (s_writer != null)
        {
            s_writer.Flush();
            s_writer.Close();
        }
    }

    private void OnDisable()
    {
        if (InspectionManager.Store_interaction && s_writer != null)
            s_writer.Close();
    }
}
