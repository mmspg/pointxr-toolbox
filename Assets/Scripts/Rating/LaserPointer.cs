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


public class LaserPointer : MonoBehaviour
{
    // Start is called before the first frame update
    private readonly float length = 5.0f;
    private LineRenderer line = null;
    

    void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    void Update()
    {
        UpdateLine();  
    }

    private RaycastHit CreatRayCast()
    {
        int layer_mask = 1 << 13; // 13 layer is the scoring board

        // Casts a ray, from controller, in direction forward, against all colliders in the scoring board layer
        Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit hit, Mathf.Infinity, layer_mask);

        return hit;
    }

    private void UpdateLine()
    {
        float target_length = length;
        RaycastHit hit = CreatRayCast();
        Vector3 target = transform.position + (transform.forward * target_length);

        if (hit.collider!=null)
        {
            // Cast ray of pointer
            line.enabled = true;
            target = hit.point;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, target);
        }
        else
        {
            line.enabled = false;
        }
    }
}
