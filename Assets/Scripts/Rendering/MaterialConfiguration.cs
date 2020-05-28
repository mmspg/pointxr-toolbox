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


public class MaterialConfiguration : MonoBehaviour
{
    // Initialization
    private Material material;
    private Shader shader;
    public static string ShaderShape;
    public static bool ShaderInterpolation;
    public static bool AdaptivePoint;
    public static float FixedPointSize;
    public static float PointScalingFactor;
    public static float ModelScalingFactor;
    public static byte[] Tint = {128, 128, 128};
    public static bool IsUpdated;


    void OnEnable()
    {
        UpdateMaterial();
        ModelScalingFactor = transform.localScale.x;
        IsUpdated = false;
    }

    void UpdateMaterial()
    {
        if (material == null || IsUpdated)
        {
            // If there is no point material with the same name as the mesh, use the default point material for rendering
            if (Resources.Load<Material>("Materials/" + GetComponent<MeshFilter>().sharedMesh.name) == null)
                GetComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>("Materials/" + "Default Point");
            else
                GetComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>("Materials/" + GetComponent<MeshFilter>().sharedMesh.name);

            material = GetComponent<MeshRenderer>().sharedMaterial;

            // Update relevant properties according to changes made in the ModelConfiguration script
            if (IsUpdated)
            {
                IsUpdated = false;
                shader = Shader.Find("Point Cloud/" + ShaderShape);                      
                material.shader = shader;                                               // Shader shape
                material.SetInt("_ShaderInterpolation", ShaderInterpolation ? 1 : 0);   // Use shader interpolation 
                material.SetInt("_AdaptivePoint", AdaptivePoint ? 1 : 0);               // Use adaptive point size mode
                material.SetFloat("_PointSize", FixedPointSize);                        // Set size of fixed point size
                material.SetFloat("_PointScalingFactor", PointScalingFactor);           // Set scaling factor for point size
                material.SetFloat("_ModelScalingFactor", ModelScalingFactor);           // Follow the transform localscale, kept unchanged;
                Color c = new Color32(Tint[0], Tint[1], Tint[2], 255); 
                material.SetColor("_Tint", c);
            }
        }
    }

    private void Update()
    {
        UpdateMaterial();
    }

    void OnDisable()
    {
        material = null;
    }

}
