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
//
// This script handles the interactive configuration of the visual appearnce of
// a model through a GUI. 
// The input, output and the shader can be selected in the Editor. Alternatively, 
// a config file can be used, through which the default model display values can  
// be additionally specified (per model). 
// The user can configure the per-point rendering parameters through the main menu
// in the Game view. The model size and scale can be adjusted in the Scene view.
// Note that camera changes in Scene view are reflected in the Game view; however,
// you can align the latter with the former. 
// In particular, in the main menu, the following options are given:
// 1. Center the model 
// 2. Discard any changes and fall back to the default rendering parameters 
// 3. Next model (useful when a config file with several models is used)
// 4. Align camera of the Game view to the last scene of the Scene view 
// 5. Reseting the camera to a fixed position that can be specified 
// 6. Save a prefab and a config file with the current rendering configurations
// The point cloud should be placed under the Resources folder and a material 
// is automatically generated. Use different input and output filenames.
//
//
// Reference:
//   E.Alexiou, N. Yang, and T.Ebrahimi, "PointXR: a toolbox for visualization
//   and subjective evaluation of point clouds in virtual reality," 2020 Twelfth
//   International Conference on Quality of Multimedia Experienence (QoMEX)




using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;


namespace Pcx
{
    public class RenderingConfiguration : MonoBehaviour
    {
        // Inspector IO variables
        [HideInInspector]
        public bool Load_config_file;   // Flag that is set true if rendering parameters are loaded from a config file
        [HideInInspector]
        public string Config_file;      // Filename of the config file. The config file is in JSON format and has a specific structure that should be followed. An output config file can be used as an input
        [HideInInspector]
        public string File_input;       // Input filename. The input should be a point cloud in PLY format, placed under the /Resources/ folder 
        [HideInInspector]
        public string File_output;      // Output filename. The filename should be unique and different from the input filename. The same filename is used for stored data (e.g., prefab, material, config)
        [HideInInspector]
        public ShaderType Shader_shape; // ShaderShape shape
        
        // IO variables
        private bool load_config;
        private string config;
        public static string input;
        public static string output;
        public static string shader;

        public static bool shader_interpolation = false;
        public static bool adaptive_point = false;
        public static int k_nn = 0;
        public static int k_nn_old = 0;
        public static float fixed_point_size = 0.8f;
        public static float point_scaling_factor = 1f;
        public static float model_scaling_factor;
        public static bool initialize_fallback_camera = true;
        public static Vector3 fallback_camera_position = new Vector3(0, 1, -2);
        public static Quaternion fallback_camera_rotation = new Quaternion(0, 0, 0, 1);

        // Variables
        public static GameObject model;
        private Mesh mesh;
        private ConfigList list;
        private int index = 0;

        // GUI
        private Rect gui = new Rect(10, 60, 205, 270);

        public enum ShaderType { Disk, Square, Point }

        // Paths
        private readonly string path_to_files = "Assets/Resources/";
        private readonly string path_to_materials = "Assets/Resources/Materials/";
        private readonly string path_to_configs = "Assets/Resources/Configs/";

        
        void OnEnable()
        {
            // Get configuration file (i.e., once per game)
            GetConfig();

            // Render model
            RenderModel();
        }

        void OnGUI()
        {
            gui = GUI.Window(1, gui, SetMenu, "Rendering configurations");
            if (model != null)
                CheckUpdates();
        }

        private void GetConfig()
        {
            load_config = Load_config_file;
            config = Config_file;
        }

        private void RenderModel()
        {
            // Get inputs
            GetInput();

           // If inputs are valid, get model
            if (ValidInput())
                model = GetModel();

            // If config file is not used, create a list to store current model rendering configurations
            if (!load_config)
            {
                list = ConfigurationList.CreateList(1);
                list.ModelConfig[0] = ConfigurationList.CreateNode();
            }

            // Set initial rendering parameters, as stored in the configuration list
            ResetRenderingConfigurations();

            // Initialize position and rotation of camera in 2D desktop view
            if (initialize_fallback_camera)
                ResetFallbackCamera();
        }

        private void GetInput()
        {
            if (!load_config)
            {
                input = File_input;
                output = File_output;
                shader = Shader_shape.ToString();
            }
            else
            {
                config = Path.GetFileNameWithoutExtension(config);
                string jsonIn = path_to_configs + config + ".json";
                if (File.Exists(jsonIn))
                {
                    list = JsonUtility.FromJson<ConfigList>(File.ReadAllText(jsonIn));

                    input = list.ModelConfig[index].Input;
                    output = list.ModelConfig[index].Output;
                    shader = list.ModelConfig[index].ShaderShape;
                }
                else
                {
                    Debug.LogError("Config file not found");
                    EditorApplication.ExecuteMenuItem("Edit/Play");
                }
            }
        }

        private bool ValidInput()
        {
            if (input.Equals(output))
            {
                Debug.LogError("Output filename should be different than input filename");
                EditorApplication.ExecuteMenuItem("Edit/Play");

                return false;
            }
            if (input.Equals("") || output.Equals(""))
            {
                Debug.LogError("Empty input and/or output filenames");
                EditorApplication.ExecuteMenuItem("Edit/Play");

                return false;
            }
            input = Path.GetFileNameWithoutExtension(input);
            if (!File.Exists(path_to_files + input + ".ply"))
            {
                Debug.LogError("Ply file not found");
                EditorApplication.ExecuteMenuItem("Edit/Play");

                return false;
            }
            if (!(shader.Equals("Disk") || shader.Equals("Square") || shader.Equals("Point")))
            {
                Debug.LogError("Shader not found");
                EditorApplication.ExecuteMenuItem("Edit/Play");

                return false;
            }

            return true;
        }

        private GameObject GetModel()
        {
            GameObject m = Instantiate(Resources.Load<GameObject>(input)) as GameObject;
            m.AddComponent<MaterialConfiguration>();    // Add MaterialConfiguration script for model rendering 

            return m;
        }

        private void ResetRenderingConfigurations()
        {
            // Set default rendering parameters to a model as specified by the user, or the config file
            shader_interpolation = list.ModelConfig[index].ShaderInterpolation;
            adaptive_point = list.ModelConfig[index].AdaptivePoint;
            k_nn = Convert.ToInt16(list.ModelConfig[index].Knn.Equals("NaN") ? "0" : list.ModelConfig[index].Knn);
            k_nn_old = k_nn;
            fixed_point_size = list.ModelConfig[index].FixedPointSize.Equals("NaN") ? 0.0f : Convert.ToSingle(list.ModelConfig[index].FixedPointSize);
            point_scaling_factor = Convert.ToSingle(list.ModelConfig[index].PointScalingFactor);

            model_scaling_factor = Convert.ToSingle(list.ModelConfig[index].ModelScalingFactor);
            float msX = Convert.ToSingle(list.ModelConfig[index].ModelScale.X);
            float msY = Convert.ToSingle(list.ModelConfig[index].ModelScale.Y);
            float msZ = Convert.ToSingle(list.ModelConfig[index].ModelScale.Z);
            model.transform.localScale = new Vector3(msX, msY, msZ);
            float mpX = Convert.ToSingle(list.ModelConfig[index].ModelPosition.X);
            float mpY = Convert.ToSingle(list.ModelConfig[index].ModelPosition.Y);
            float mpZ = Convert.ToSingle(list.ModelConfig[index].ModelPosition.Z);
            model.transform.localPosition = new Vector3(mpX, mpY, mpZ);
            float mrX = Convert.ToSingle(list.ModelConfig[index].ModelRotation.X);
            float mrY = Convert.ToSingle(list.ModelConfig[index].ModelRotation.Y);
            float mrZ = Convert.ToSingle(list.ModelConfig[index].ModelRotation.Z);
            model.transform.eulerAngles = new Vector3(mrX, mrY, mrZ);

            SetMaterial();
        }

        private void ResetFallbackCamera()
        {
            // Reset camera in 2D desktop view in default position and orientation
            GameObject cam = GameObject.Find("FallbackObjects");
            cam.transform.SetPositionAndRotation(fallback_camera_position, fallback_camera_rotation);
        }

        void SetMenu(int windowID)
        {
            // Point shader doesn't have shader_interpolation, adaptive/fixedPoint, or point_scaling_factor properties
            if (!shader.Equals("Point"))
            {
                // Set model rendering parameters based on user's input through GUI 
                shader_interpolation = GUI.Toggle(new Rect(10, 25, 150, 20), shader_interpolation, " Shader interpolation");
                adaptive_point = GUI.Toggle(new Rect(10, 55, 150, 20), adaptive_point, " Adaptive point");
                if (adaptive_point)
                {
                    // Adaptive point size entry
                    GUI.Label(new Rect(30, 85, 125, 20), "K-nearest neighbors: ");
                    string k_str = GUI.TextField(new Rect(155, 85, 40, 25), "" + k_nn);
                    k_nn = Convert.ToInt16(k_str);

                    if (GUI.Button(new Rect(65, 110, 55, 25), "Apply"))
                    {
                        // If the provided k_nn value is the same as the previous k_nn value, there is no need to re-import the model. 
                        // During importing, local distances between each point and its K-nearest neighbors are computed (see the paper). 
                        // Based on these distances, the size of each point is determined. 
                        // During this procedure, outlier points are identified and a fixed size based on intrinsic resolution is set.
                        // NOTE that this processing might be time-consuming if a large k_nn value is used
                        if (k_nn != k_nn_old)
                        {
                            ReloadPointCloud();
                            SetMaterial();
                        }
                    }
                }
                else
                {
                    // Fixed point size entry
                    GUI.Label(new Rect(30, 85, 125, 20), "Fixed point size: ");
                    fixed_point_size = Mathf.Round(fixed_point_size * 1000f) / 1000f;       // Set 3 decimal digits
                    fixed_point_size = GUI.HorizontalSlider(new Rect(30, 110, 125, 30), fixed_point_size, 0.0f, 5f);
                    string f_str = GUI.TextField(new Rect(155, 85, 40, 25), "" + fixed_point_size.ToString("0.000"));
                    fixed_point_size = (Convert.ToSingle(f_str) <= 5 && Convert.ToSingle(f_str) >= 0) ? Convert.ToSingle(f_str) : 0.80f;
                }

                // Point scaling factor entry
                GUI.Label(new Rect(30, 140, 125, 20), "Point scaling factor: ");
                point_scaling_factor = Mathf.Round(point_scaling_factor * 1000f) / 1000f;   // Set 3 decimal digits
                point_scaling_factor = GUI.HorizontalSlider(new Rect(30, 165, 125, 30), point_scaling_factor, 0.0f, 2f);
                string p_str = GUI.TextField(new Rect(155, 140, 40, 25), "" + point_scaling_factor.ToString("0.000"));
                point_scaling_factor = (Convert.ToSingle(p_str) <= 2 && Convert.ToSingle(p_str) >= 0) ? Convert.ToSingle(p_str) : 1.000f;
            }

            // Center the model of the camera view after changes in the transform setting under manual adjustment in scene mode
            if (GUI.Button(new Rect(7, 195, 59, 30), "Center"))
                OnPressCenter();

            // Reset to initial rendering settings of the model
            if (GUI.Button(new Rect(73, 195, 59, 30), "Discard"))
                OnPressDiscard();

            // If there are more than one models in the configuration list
            if (GUI.Button(new Rect(139, 195, 59, 30), "Next"))
                OnPressNext();

            // If in 2D debug mode (i.e., camera in 2D desktop view is not null)
            GameObject cam = GameObject.Find("FallbackObjects");
            if (cam != null)
                ShowAlignCamera(cam);

            // Save point material, prefab and rendering configurations
            if (GUI.Button(new Rect(73, 230, 59, 30), "Reset"))
                OnPressReset();

            // Save point material, prefab and rendering configurations
            if (GUI.Button(new Rect(139, 230, 59, 30), "Save"))
                OnPressSave();
        }

        private void ReloadPointCloud()
        {
            // Re-import model in case the k-nn is modified in adaptive rendering mode
            mesh = ImportAsMesh(path_to_files + input + ".ply", k_nn);
            model.GetComponent<MeshFilter>().sharedMesh = mesh;
            k_nn_old = k_nn;

            var mat = Instantiate(AssetDatabase.LoadAssetAtPath<Material>(path_to_materials + input + ".mat")) as Material;
            model.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        private void OnPressCenter()
        {
            CenterModel();
        }

        private void CenterModel()
        {
            Vector3 t1 = new Vector3(model.GetComponent<MeshRenderer>().bounds.center.x, 0f, model.GetComponent<MeshRenderer>().bounds.center.z);
            model.transform.localPosition -= t1;    // adjust x and z
            Vector3 t2 = new Vector3(model.transform.localPosition.x, 0f, model.transform.localPosition.z);
            model.transform.localPosition = t2;     // set y to 0 to lift up the model 
        }

        private void OnPressDiscard()
        {
            ResetRenderingConfigurations();
        }

        private void OnPressNext()
        {
            ShowNextModel();
        }

        private void ShowNextModel()
        {
            Destroy(model);
            if (index < list.ModelConfig.Length - 1)
            {
                index += 1;
                RenderModel();
            }
            else
            {
                EditorApplication.ExecuteMenuItem("Edit/Play");
            }
        }

        private void ShowAlignCamera(GameObject camera)
        {
            // Show align button for camera alignment only in 2D desktop view 
            if (GUI.Button(new Rect(7, 230, 59, 30), "Align"))
                OnPressAlign(camera);
        }

        private void OnPressAlign(GameObject camera)
        {
            AlignCamera(camera);
        }

        private void AlignCamera(GameObject cam)
        {
            // Align camera in Game mode with camera in Scene mode
            SceneView sview = SceneView.lastActiveSceneView;
            cam.transform.SetPositionAndRotation(sview.camera.transform.localPosition, sview.camera.transform.localRotation);
        }

        private void OnPressReset()
        {
            ResetFallbackCamera();
        }

        private void OnPressSave()
        {
            StoreFiles();
        }

        private void StoreFiles()
        {
            // Point material
            if (!File.Exists(path_to_materials + output + ".mat"))
                StoreMaterial();
            else
                Debug.LogError("Material not saved. File already exists");

            // Prefab model
            if (!File.Exists(path_to_files + output + ".prefab"))
                StorePrefab();
            else
                Debug.LogError("Prefab not saved. File already exists");

            // Rendering configurations in JSON file
            if (!File.Exists(path_to_configs + output + ".json"))
                StoreConfig();
            else
                Debug.LogError("Json not saved. File already exists");
        }

        private void StoreMaterial()
        {
            var mat = new Material(model.GetComponent<MeshRenderer>().sharedMaterial);
            AssetDatabase.CreateAsset(mat, path_to_materials + output + ".mat");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void StorePrefab()
        {
            model.GetComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(path_to_materials + output + ".mat"); ;
            PrefabUtility.SaveAsPrefabAsset(model, path_to_files + output + ".prefab");
        }

        private void StoreConfig()
        {
            ConfigList clist = ConfigurationList.CreateList(1);
            clist.ModelConfig[0] = ConfigurationList.CreateNode();

            string jsonOut = path_to_configs + output + ".json";
            File.WriteAllText(jsonOut, JsonUtility.ToJson(clist));
        }

        private void CheckUpdates()
        {
            // User can modify the output filename and the shader during the game, only when a configuration file is not used
            if (!load_config)
            {
                output = File_output;
                shader = Shader_shape.ToString();
            }

            model_scaling_factor = model.transform.localScale.x;
            if (UpdatedModel())
                SetMaterial();
        }

        private bool UpdatedModel()
        {
            return shader != MaterialConfiguration.ShaderShape ||
                shader_interpolation != MaterialConfiguration.ShaderInterpolation ||
                adaptive_point != MaterialConfiguration.AdaptivePoint ||
                fixed_point_size != MaterialConfiguration.FixedPointSize ||
                point_scaling_factor != MaterialConfiguration.PointScalingFactor ||
                model_scaling_factor != MaterialConfiguration.ModelScalingFactor;
        }

        public static void SetMaterial()
        {
            MaterialConfiguration.ShaderShape = shader;
            MaterialConfiguration.ShaderInterpolation = shader_interpolation;
            MaterialConfiguration.AdaptivePoint = adaptive_point;
            MaterialConfiguration.FixedPointSize = fixed_point_size;
            MaterialConfiguration.PointScalingFactor = point_scaling_factor;
            MaterialConfiguration.ModelScalingFactor = model_scaling_factor;
            MaterialConfiguration.IsUpdated = true;
        }


        // Customize inspector GUI
#if UNITY_EDITOR
        [CustomEditor(typeof(RenderingConfiguration)), CanEditMultipleObjects]
        public class ModelConfigure_Editor : Editor
        {

            public override void OnInspectorGUI()
            {
                DrawDefaultInspector(); // for other non-HideInInspector fields

                RenderingConfiguration script = (RenderingConfiguration)target;
                script.Load_config_file = EditorGUILayout.Toggle("Load config file", script.Load_config_file);

                // If flag is true, show other fields
                if (script.Load_config_file)
                {
                    script.Config_file = EditorGUILayout.TextField("Config filename", script.Config_file);
                }
                else
                {
                    script.File_input = EditorGUILayout.TextField("Input filename", script.File_input);
                    script.File_output = EditorGUILayout.TextField("Output filename", script.File_output);
                    script.Shader_shape = (ShaderType)EditorGUILayout.EnumPopup("Shader", script.Shader_shape);
                }
            }
        }
#endif

        // The code below is from Pcx importer with modification on ImportAsMesh function (i.e., adding one variable k_nn)
        #region Internal data structure

        enum DataProperty
        {
            Invalid,
            X, Y, Z,
            R, G, B, A,
            Data8, Data16, Data32
        }

        static int GetPropertySize(DataProperty p)
        {
            switch (p)
            {
                case DataProperty.X: return 4;
                case DataProperty.Y: return 4;
                case DataProperty.Z: return 4;
                case DataProperty.R: return 1;
                case DataProperty.G: return 1;
                case DataProperty.B: return 1;
                case DataProperty.A: return 1;
                //case DataProperty.Size: return 4;
                case DataProperty.Data8: return 1;
                case DataProperty.Data16: return 2;
                case DataProperty.Data32: return 4;
            }
            return 0;
        }

        class DataHeader
        {
            public List<DataProperty> properties = new List<DataProperty>();
            public int vertexCount = -1;
        }

        class DataBody
        {
            public List<Vector3> vertices;
            public List<Color32> colors;
            public List<Vector2> uv;
            public static Vector3[] vertArr;

            public DataBody(int vertexCount)
            {
                vertices = new List<Vector3>(vertexCount);
                colors = new List<Color32>(vertexCount);
                uv = new List<Vector2>(vertexCount);
            }

            public void AddPoint(
                float x, float y, float z,
                byte r, byte g, byte b, byte a
            )
            {
                vertices.Add(new Vector3(x, y, z));
                colors.Add(new Color32(r, g, b, a));
                //uv.Add(new Vector2(size, size));
            }

            public void CalculatePointSize(int vertexCount, int KNN)
            {
                vertArr = vertices.ToArray();
                KDTree tree = KDTree.MakeFromPoints(vertArr);
                float[] pointSize = new float[vertexCount];
                for (int i = 0; i < vertexCount; i++)
                {
                    int[] indices = tree.FindNearestsK(vertArr[i], KNN); // 5 nearest neighbours
                    float[] dists = new float[KNN];
                    for (int j = 0; j < KNN; j++)
                    {
                        dists[j] = Vector3.Distance(vertArr[i], vertArr[indices[j]]);  // calculate the point size 
                    }
                    pointSize[i] = dists.Average();
                }
                float globalMean = pointSize.Average();

                float globalSTD = Mathf.Sqrt(pointSize.Select(val => (val - globalMean) * (val - globalMean)).Sum() / vertexCount); // global std calculation
                for (int i = 0; i < vertexCount; i++)
                {
                    if (pointSize[i] > globalMean + 3f * globalSTD || pointSize[i] < globalMean - 3f * globalSTD) // clamp to one range
                    {
                        pointSize[i] = globalMean;
                    }
                    uv.Add(new Vector2(pointSize[i], pointSize[i]));  // uv computation 
                }

            }
        }

        #endregion
        #region Reader implementation

        Mesh ImportAsMesh(string path, int KNN)
        {
            try
            {
                var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                var header = ReadDataHeader(new StreamReader(stream));
                var body = ReadDataBody(header, new BinaryReader(stream), KNN);
                //this.vertexCount = header.vertexCount;

                var mesh = new Mesh();
                mesh.name = Path.GetFileNameWithoutExtension(path);

                mesh.indexFormat = header.vertexCount > 65535 ?
                    IndexFormat.UInt32 : IndexFormat.UInt16;   // different data

                mesh.SetVertices(body.vertices);
                mesh.SetColors(body.colors);
                mesh.SetUVs(0, body.uv);

                mesh.SetIndices(
                    Enumerable.Range(0, header.vertexCount).ToArray(),
                    MeshTopology.Points, 0
                );

                mesh.UploadMeshData(true);
                return mesh;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed importing " + path + ". " + e.Message);
                return null;
            }
        }

        static DataHeader ReadDataHeader(StreamReader reader)
        {
            var data = new DataHeader();
            var readCount = 0;

            // Magic number line ("ply")
            var line = reader.ReadLine();
            readCount += line.Length + 1;
            if (line != "ply")
                throw new ArgumentException("Magic number ('ply') mismatch.");

            // Data format: check if it's binary/little endian.
            line = reader.ReadLine();
            readCount += line.Length + 1;
            if (line != "format binary_little_endian 1.0")
                throw new ArgumentException(
                    "Invalid data format ('" + line + "'). " +
                    "Should be binary/little endian.");

            // Read header contents.
            for (var skip = false; ;)
            {
                // Read a line and split it with white space.
                line = reader.ReadLine();
                readCount += line.Length + 1;
                if (line == "end_header") break;
                var col = line.Split();

                // Element declaration (unskippable)
                if (col[0] == "element")
                {
                    if (col[1] == "vertex")
                    {
                        data.vertexCount = Convert.ToInt32(col[2]);
                        skip = false;
                    }
                    else
                    {
                        // Don't read elements other than vertices.
                        skip = true;
                    }
                }

                if (skip) continue; // next line 

                // Property declaration line
                if (col[0] == "property")
                {
                    var prop = DataProperty.Invalid;

                    // Parse the property name entry.
                    switch (col[2])
                    {
                        case "x": prop = DataProperty.X; break;
                        case "y": prop = DataProperty.Y; break;
                        case "z": prop = DataProperty.Z; break;
                        case "red": prop = DataProperty.R; break;
                        case "green": prop = DataProperty.G; break;
                        case "blue": prop = DataProperty.B; break;
                        case "alpha": prop = DataProperty.A; break;
                            //case "intensity" : prop = DataProperty.Size; break;
                    }

                    // Check the property type.
                    if (col[1] == "char" || col[1] == "uchar")
                    {
                        if (prop == DataProperty.Invalid)
                            prop = DataProperty.Data8;
                        else if (GetPropertySize(prop) != 1)
                            throw new ArgumentException("Invalid property type ('" + line + "').");
                    }
                    else if (col[1] == "short" || col[1] == "ushort")
                    {
                        if (prop == DataProperty.Invalid)
                            prop = DataProperty.Data16;
                        else if (GetPropertySize(prop) != 2)
                            throw new ArgumentException("Invalid property type ('" + line + "').");
                    }
                    else if (col[1] == "int" || col[1] == "uint" || col[1] == "float")
                    {
                        if (prop == DataProperty.Invalid)
                            prop = DataProperty.Data32;
                        else if (GetPropertySize(prop) != 4)
                            throw new ArgumentException("Invalid property type ('" + line + "').");
                    }
                    else
                    {
                        throw new ArgumentException("Unsupported property type ('" + line + "').");
                    }
                    data.properties.Add(prop);
                }
            }

            // Rewind the stream back to the exact position of the reader.
            reader.BaseStream.Position = readCount;

            return data;
        }

        DataBody ReadDataBody(DataHeader header, BinaryReader reader, int KNN)
        {
            var data = new DataBody(header.vertexCount);

            float x = 0, y = 0, z = 0;
            Byte r = 255, g = 255, b = 255, a = 255;

            for (var i = 0; i < header.vertexCount; i++)
            {
                foreach (var prop in header.properties)
                {
                    switch (prop)
                    {
                        case DataProperty.X: x = reader.ReadSingle(); break;
                        case DataProperty.Y: y = reader.ReadSingle(); break;
                        case DataProperty.Z: z = reader.ReadSingle(); break;

                        case DataProperty.R: r = reader.ReadByte(); break;
                        case DataProperty.G: g = reader.ReadByte(); break;
                        case DataProperty.B: b = reader.ReadByte(); break;
                        case DataProperty.A: a = reader.ReadByte(); break;

                        //case DataProperty.Size: size = reader.ReadSingle(); break;
                        case DataProperty.Data8: reader.ReadByte(); break;
                        case DataProperty.Data16: reader.BaseStream.Position += 2; break;
                        case DataProperty.Data32: reader.BaseStream.Position += 4; break;
                    }
                }

                data.AddPoint(x, y, z, r, g, b, a);

            }
            if (KNN > 0)
                data.CalculatePointSize(header.vertexCount, KNN);

            return data;
        }
        #endregion
    }
}
