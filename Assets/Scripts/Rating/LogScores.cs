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
using System.IO;


public class LogScores : MonoBehaviour
{    
    private static StreamWriter s_writer;


    void Awake()
    {
        if (InspectionManager.Store_scores)
            OpenFile();            
    }

    private void OpenFile()
    {
        // Note that the same file is used for the entire batch
        DirectoryInfo dir;
        dir = Directory.CreateDirectory(InspectionManager.path_to_logs + PlayerPrefs.GetString("_Batch"));
        s_writer = new StreamWriter(dir.FullName + "/" + "_Scores.csv", true);

        if (PlayerPrefs.GetInt("_StartSession") == 1)
        {
            s_writer.WriteLine("Date:" + System.DateTime.Now.ToString("yyyyMMdd") + PlayerPrefs.GetString("_StartTime").ToString());
            s_writer.WriteLine("Batch:" + PlayerPrefs.GetString("_Batch"));
            s_writer.WriteLine("Protocol:" + PlayerPrefs.GetString("_Protocol"));
            s_writer.WriteLine("step,score,start,end");
            s_writer.Flush();
            PlayerPrefs.SetInt("_StartSession", 0);
        }
    }

    public static void StoreData(string score)
    {
        s_writer.WriteLine("   " + PlayerPrefs.GetInt("_Step").ToString() + "," + score.ToString() + "," + PlayerPrefs.GetString("_StartTime").ToString() + "," + PlayerPrefs.GetString("_EndTime").ToString());
        s_writer.Flush();
    }

    private void OnDisable()
    {
        if (s_writer != null)
        {
            s_writer.Flush();
            s_writer.Close();
        }
    }
}
