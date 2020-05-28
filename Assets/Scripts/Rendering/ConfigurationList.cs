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


[System.Serializable]
public struct ModelScale
{
    public string X;
    public string Y;
    public string Z;
}

[System.Serializable]
public struct ModelPosition
{
    public string X;
    public string Y;
    public string Z;
}

[System.Serializable]
public struct ModelRotation
{
    public string X;
    public string Y;
    public string Z;
}

[System.Serializable]
public struct ConfigNode
{
    public string Input;
    public string Output;
    public string ShaderShape;
    public bool ShaderInterpolation;
    public bool AdaptivePoint;
    public string FixedPointSize;
    public string Knn;
    public string PointScalingFactor;
    public string ModelScalingFactor;
    public ModelScale ModelScale;
    public ModelPosition ModelPosition;
    public ModelRotation ModelRotation;
}

[System.Serializable]
public struct ConfigList
{
    public ConfigNode[] ModelConfig;
}


public class ConfigurationList : MonoBehaviour
{
    // Create list to store rendering configurations
    public static ConfigList CreateList(int size)
    {
        ConfigList clist = new ConfigList
        {
            ModelConfig = new ConfigNode[size]
        };

        return clist;
    }

    // Create a node with current rendering configurations
    public static ConfigNode CreateNode()
    {
        ConfigNode node = new ConfigNode
        {
            Input = Pcx.RenderingConfiguration.input,
            Output = Pcx.RenderingConfiguration.output,
            ShaderShape = Pcx.RenderingConfiguration.shader,
            ShaderInterpolation = Pcx.RenderingConfiguration.shader_interpolation,
            AdaptivePoint = Pcx.RenderingConfiguration.adaptive_point,
        };

        if (Pcx.RenderingConfiguration.shader.Equals("Disk") || Pcx.RenderingConfiguration.shader.Equals("Square"))
        {
            node.PointScalingFactor = Pcx.RenderingConfiguration.point_scaling_factor.ToString();
            if (Pcx.RenderingConfiguration.adaptive_point)
            {
                node.Knn = Pcx.RenderingConfiguration.k_nn.ToString();
                node.FixedPointSize = "NaN";
            }
            else
            {
                node.Knn = "NaN";
                node.FixedPointSize = Pcx.RenderingConfiguration.fixed_point_size.ToString();
            }
        }
        else
        {
            node.PointScalingFactor = "NaN";
            node.Knn = "NaN";
            node.FixedPointSize = "NaN";
        }

        ModelScale ms = new ModelScale
        {
            X = Pcx.RenderingConfiguration.model.transform.localScale.x.ToString(),
            Y = Pcx.RenderingConfiguration.model.transform.localScale.y.ToString(),
            Z = Pcx.RenderingConfiguration.model.transform.localScale.z.ToString()
        };
        node.ModelScale = ms;

        if (UniformScaling(ms))
            node.ModelScalingFactor = Pcx.RenderingConfiguration.model.transform.localScale.x.ToString();
        else
            node.ModelScalingFactor = "NaN";

        ModelPosition mc = new ModelPosition
        {
            X = Pcx.RenderingConfiguration.model.transform.localPosition.x.ToString(),
            Y = Pcx.RenderingConfiguration.model.transform.localPosition.y.ToString(),
            Z = Pcx.RenderingConfiguration.model.transform.localPosition.z.ToString()
        };
        node.ModelPosition = mc;

        ModelRotation mr = new ModelRotation
        {
            X = Pcx.RenderingConfiguration.model.transform.eulerAngles.x.ToString(),
            Y = Pcx.RenderingConfiguration.model.transform.eulerAngles.y.ToString(),
            Z = Pcx.RenderingConfiguration.model.transform.eulerAngles.z.ToString()
        };
        node.ModelRotation = mr;

        return node;
    }

    // Check if the model is scaled uniformly across all dimensions
    private static bool UniformScaling(ModelScale ms)
    {
        if (ms.X.Equals(ms.Y) && ms.X.Equals(ms.Z) && ms.Y.Equals(ms.Z))
            return true;
        else
            return false;
    }
}
