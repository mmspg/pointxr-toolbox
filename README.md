
# PointXR toolbox


In this repository, we release the **PointXR toolbox**. A software that is built in Unity for rendering and visualization of 3D point clouds in virtual environments, as part of the [**PointXR**]([https://www.epfl.ch/labs/mmspg/downloads/pointxr/]) [1]. The toolbox consists of a set of 3D Unity scenes, developed to: (a) adjust the visual appearance of point clouds by enabling different rendering configurations, and (b) host experiments under variants of interactive and passive evaluation protocols.

In particular, the toolbox consists of 4 different scenes that can operate independently:

 1. Rendering: Scene for interactive configuration of the visual appearance of a model through a Graphical User Interface. The rendering pipeline depends on the default [Pcx point cloud importer and renderer for Unity](https://github.com/keijiro/Pcx), which is enhanced with: (i) adaptive point size mode, (ii) square shader, and (iii) shader interpolation.
 2. Inspection: Scene for the selection of the preferred subjective evaluation protocol (e.g., single stimulus, double stimulus) and variant (e.g., simultaneous, sequential). Additional options, such as model rating, or recording the observer's interactions can be enabled or disabled.  
 3. Rating: Scene for configuration of the rating panel and the grading scale. The questions and answers that are shown to the observers can be specified.
 4. Capturing: Scene for producing videos using a virtual camera that captures the model from pre-defined paths (to be released soon).

Below, several screenshots of the same model loaded in the Rendering scene are illustrated, after applying different rendering configurations:

![alt text](/docs/screenshot.png)


### Dependencies

The [Pcx point cloud importer and renderer for Unity](https://github.com/keijiro/Pcx) is a dependency, which is coming together with the PointXR toolbox.


### System requirements

The software is built and developed in Unity 2019.3. It has not been extensively tested with other Unity versions. In case you face issues with your current version, [Unity Hub](https://docs.unity3d.com/Manual/GettingStartedInstallingHub.html) might come in handy.


### Application

The **PointXR toolbox** was developed in the framework of [1], in addition to the generation of the **PointXR dataset**, and the **PointXR experimental data**. In the latter, we combine the use of the software and the dataset in a subjective quality evaluation study allowing 6 degrees of freedom interactions in Virtual Reality using double-stimulus protocol variants. For more information regarding the experiment, the models, and the rendering configurations, the reader can refer to [1].


### Conditions of use

If you wish to use this software in your research, we kindly ask you to cite [1].


### References

[1] E. Alexiou, N. Yang, and T. Ebrahimi, "[PointXR: A toolbox for visualization and subjective evaluation of point clouds in virtual reality](https://infoscience.epfl.ch/record/277378?ln=en)," 2020 Twelfth International Conference on Quality of Multimedia Experience (QoMEX), Athlone, 2020.
