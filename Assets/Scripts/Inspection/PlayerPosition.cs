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
using Valve.VR.InteractionSystem;

public class PlayerPosition : MonoBehaviour
{
    private readonly Vector3 fixed_spot = new Vector3(0, 0, -2);
    private readonly float radius_min = 1;
    private readonly float radius_max = 4;

    private Modes mode;
    private enum Modes { Random, Fixed, None }

    private float x;
    private float z;
    

    private void Awake()
    {
        mode = Modes.Fixed;
        x = Random.Range(-radius_max, radius_max);
        z = Random.Range(-radius_max, radius_max);
    }

    void Start()
    {
        // Randomize player position. The current implementation doesn't modify the orientation
        if (mode == Modes.Random)
        {
            while (x < radius_min && x > -radius_min)
                x = Random.Range(-radius_max, radius_max);

            while (z < radius_min && z > -radius_min)
                z = Random.Range(-radius_max, radius_max);

            Player.instance.transform.position = new Vector3(x, 0, z);
        }
        else if (mode == Modes.Fixed)
        {
            Player.instance.transform.position = fixed_spot;
        }
    }
}
