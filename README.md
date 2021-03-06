# Three-Dimensional Interactive Visualization of a Thematic Network of Immersive Learning Research
This repository contains the Unity code and database to the artifact produced in the scope of the Master's Thesis in Multimedia "Three-Dimensional Interactive Visualization of a Thematic Network of Immersive Learning Research" by Madalena Sousa from the Faculty of Engineering of University of Porto.

## Abstract
The field of Immersive Learning Research crosses multiple scientific areas. Awareness of the interconnections of this interdisciplinary literature with standard scholarly tools is difficult. This work provides a three-dimensional interactive thematic network visualization tool for this field. It allows researchers to find which works are associated with a theme within immersive learning research, see how themes are interconnected, and do filtering by article metadata. Moreover, it enables researchers to compare theme organization by visualizing different clustering criteria. The tool was developed through application of the Design Science Research methodology, addressing theme-analysis problems found via a literature survey. Research was previously conducted providing themes data on Immersive Learning practices and strategies. These data were now mapped to its source articles as part of the current effort. An interactive node-link thematic 3D network was created to visualize this data and it's relationships. By providing clustering, filtering, and 3D navigation features, the tool enables interactive visualization of the interconnection of the themes in the field of Immersive Learning Research, and exploration of its grounding papers. Any researcher can now filter by authors, publication outlets, institutions, and year, to analyze how themes were more or less explored/cited, and compare clustering criteria such as citation counts or total papers per theme, to acquire a clearer awareness of the field.

## Set Up
To run the program, follow the described steps:

1. Install [Unity 2020.3.29f1](https://unity3d.com/pt/get-unity/download) or higher, if you don't have it installed already;
2. Download the repository files;
3. Use Unity Hub to find the folder with the files and open that folder as a Unity project;
4. Click on the blue play button at the top to run the visualization.

> **Note:** If, when the project opens, you see an error in the console regarding the NuGet package, clear the error and proceed normally.

## Some Important Notes
The branch that contains the finalized code is the **main** branch. The Tests branch is where new tests of functionalities where made unitil they worked well enough to go into the main branch. The TestTimeClusters branch was used to text cluster adaptation when setting a time interval, which didn't work as expected and required more exploration, therefore this branch is outdated.

If you try to build the project and run on the web, you'll likely run into an impass. This issue occurs due to the database. Solving this issue, meaning, posting the project online, is not within the scope of the project.
