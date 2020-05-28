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


public class LoadObjects : MonoBehaviour
{
    public static GameObject Model_reference
    { get; set; }
    public static GameObject Model_distorted
    { get; set; }
    public static GameObject Sign_reference
    { get; set; }
    public static GameObject Sign_distorted
    { get; set; }
    public static GameObject Stage
    { get; set; }
    public static GameObject Stage_distorted
    { get; set; }
    public static GameObject Stage_reference
    { get; set; }

    private static readonly float sign_offset_Z = 0.5f;  // distance offset from the front side of the stimulus
    private static readonly float models_offset_X = 0.5f; // distance offset from the original position of the stimulus when double_stimulus-simultaneous is used
    private static readonly float stage_height = 0.75f;
    private static float stage_scaling_X = 1.0f;
    private static float stage_scaling_Z = 1.5f;


    public static void GetNext()
    {
        if (PlayerPrefs.GetString("_Protocol").Contains("Single"))
        {
            Model_distorted = LoadModel(Model_distorted, InspectionManager.Session.Steps[PlayerPrefs.GetInt("_Step")].Distorted + ".prefab");
            if (InspectionManager.Session.Steps[PlayerPrefs.GetInt("_Step")].Stage)
            {
                // Get bounding box of model
                Bounds bounds = GetBoundingBox(Model_distorted);

                // Adjust scale and size of Stage per model
                float stage_width = bounds.size.x * stage_scaling_X;
                float stage_depth = bounds.size.z * stage_scaling_Z;
                Vector3 scale = new Vector3(stage_width, stage_height, stage_depth);
                Vector3 shift = new Vector3(0, 0, 0);
                Stage = LoadStage(Stage, scale, shift);

                // Place models on top of the Stage
                Model_distorted.transform.position += Vector3.up * (stage_height);
            }
        }
        else if (PlayerPrefs.GetString("_Protocol").Contains("Alternating") || PlayerPrefs.GetString("_Protocol").Contains("Sequential"))
        {
            Model_reference = LoadModel(Model_reference, InspectionManager.Session.Steps[PlayerPrefs.GetInt("_Step")].Reference);
            Model_distorted = LoadModel(Model_distorted, InspectionManager.Session.Steps[PlayerPrefs.GetInt("_Step")].Distorted);

            // Get bounding box of model
            Bounds bounds = GetBoundingBox(Model_reference);

            float depth;
            if (InspectionManager.Session.Steps[PlayerPrefs.GetInt("_Step")].Stage)
            {
                // Adjust scale and size of Stage per model
                float stage_width = bounds.size.x * stage_scaling_X;
                float stage_depth = bounds.size.z * stage_scaling_Z;
                Vector3 scale = new Vector3(stage_width, stage_height, stage_depth);
                Vector3 shift = new Vector3(0, 0, 0);
                Stage = LoadStage(Stage, scale, shift); // Use unique Stage for both models (feels more natural)

                // Place models on top of the Stage
                Model_reference.transform.position += Vector3.up * (stage_height);
                Model_distorted.transform.position += Vector3.up * (stage_height);

                depth = stage_depth/2;
            }
            else
            {
                depth = Mathf.Abs(bounds.extents.z);
            }

            // Shift signs in front of the models
            float sign_depth = depth + sign_offset_Z;
            Vector3 shift_sign = new Vector3(0f, 0f, -sign_depth);
            Sign_reference = LoadSign(Sign_reference, "SignReference", shift_sign);
            Sign_distorted = LoadSign(Sign_distorted, "SignDistorted", shift_sign);

            Model_distorted.SetActive(false);
            if (Stage_distorted != null)
                Stage_distorted.SetActive(false);
            Sign_distorted.SetActive(false);
        }
        else if (PlayerPrefs.GetString("_Protocol").Contains("Simultaneous"))
        {
            Model_reference = LoadModel(Model_reference, InspectionManager.Session.Steps[PlayerPrefs.GetInt("_Step")].Reference);
            Model_distorted = LoadModel(Model_distorted, InspectionManager.Session.Steps[PlayerPrefs.GetInt("_Step")].Distorted);

            // Get bounding box of model
            Bounds bounds = GetBoundingBox(Model_reference);

            // Shift models that are displayed simultaneously left and right
            float objects_shift_X = bounds.extents.x + models_offset_X;
            Model_reference.transform.position -= new Vector3(objects_shift_X, 0, 0);
            Model_distorted.transform.position += new Vector3(objects_shift_X, 0, 0);

            float depth;
            if (InspectionManager.Session.Steps[PlayerPrefs.GetInt("_Step")].Stage)
            {
                // Adjust scale and size of Stage per model
                float stage_width = bounds.size.x * stage_scaling_X;
                float stage_depth = bounds.size.z * stage_scaling_Z;
                Vector3 scale = new Vector3(stage_width, stage_height, stage_depth);
                Vector3 shift_stage_reference = new Vector3(-objects_shift_X, 0, 0);
                Vector3 shift_stage_distorted = new Vector3(objects_shift_X, 0, 0);
                Stage_reference = LoadStage(Stage_reference, scale, shift_stage_reference);
                Stage_distorted = LoadStage(Stage_distorted, scale, shift_stage_distorted);

                // Place models on top of the stages
                Model_reference.transform.position += Vector3.up * (stage_height);
                Model_distorted.transform.position += Vector3.up * (stage_height);

                depth = stage_depth/2;
            }
            else
            {
                depth = Mathf.Abs(bounds.extents.z);
            }

            // Shift signs in front of the models
            float sign_depth = depth + sign_offset_Z;
            Vector3 shift_sign_reference = new Vector3(-objects_shift_X, 0f, -sign_depth);
            Vector3 shift_sign_distorted = new Vector3(objects_shift_X, 0f, -sign_depth);
            Sign_reference = LoadSign(Sign_reference, "SignReference", shift_sign_reference);
            Sign_distorted = LoadSign(Sign_distorted, "SignDistorted", shift_sign_distorted);
        }

        PlayerPrefs.SetString("_StartTime", System.DateTime.Now.ToString("HHmmssfff"));
    }


    private static GameObject LoadModel(GameObject model, string filename)
    {
        if (model != null)
            Destroy(model);

        model = Instantiate(Resources.Load<GameObject>(filename)) as GameObject;
        return model;
    }

    private static GameObject LoadStage(GameObject Stage, Vector3 scale, Vector3 shift)
    {
        if (Stage != null)
            Destroy(Stage);

        Stage = Instantiate(Resources.Load<GameObject>("Stage"));
        Stage.transform.localScale = scale;
        Stage.transform.position = Vector3.up * stage_height / 2.0f;
        Stage.transform.localPosition += shift;

        return Stage;
    }

    private static GameObject LoadSign(GameObject sign, string filename, Vector3 shift)
    {
        if (sign != null)
            Destroy(sign);

        sign = Instantiate(Resources.Load<GameObject>(filename));
        sign.transform.localPosition += shift;

        return sign;
    }

    private static Bounds GetBoundingBox(GameObject model)
    {
        Bounds b = model.GetComponent<MeshRenderer>().bounds;

        return b;
    }
}
