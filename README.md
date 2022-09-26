<h1 align="center">  
 <br>
 <img src="https://user-images.githubusercontent.com/73669610/192280334-c2d08401-a3cd-4cf5-9a59-95f3c1c58ee6.PNG">
  Unity-Folder-Icons
</h1>

<p align="center">
 <a href="https://unity3d.com/get-unity/download">
 <img src="https://img.shields.io/badge/unity-2021.3%2B-blue.svg" alt="Unity Download Link">
 <a href="https://github.com/WooshiiDev/Unity-Folder-Icons/blob/main/LICENSE">
 <img src="https://img.shields.io/badge/License-MIT-brightgreen.svg" alt="License MIT">
</p>
   
 <p align="center">
  <a href="#about">About</a> •
  <a href="#installation">Installation</a> •
  <a href="#features">Features</a> •
  <a href="#support">Support</a> •
  <a href="#license">License</a>
</p>
  
## About

A small extension for unity adding custom folder textures and overlay icons. This came about after discussing the limitations of what Unity already provides in terms of customisation in it's Editor. 

Some of the code does need refined/seperated (more so for future additions), and there are many, many features I'd like to personally add and see improved when I work on this.

## Installation

<p align="center">
  <a href="https://github.com/WooshiiDev/Unity-Folder-Icons/releases">Releases</a> • <a href="https://github.com/WooshiiDev/Unity-Folder-Icons/releases/download/0.1.2/Folder.Icons.v0.1.2.unitypackage">Unity Package</a> • <a href="https://github.com/WooshiiDev/Unity-Folder-Icons/archive/0.1.2.zip">Zip</a>
</p>



FolderIcons can also be installed directly through the git url
```
https://github.com/WooshiiDev/Unity-Folder-Icons.git
```

**Note:**<br>
If installed through git, to access the premade folder textures, there will be an import sample button in the package manager.
<br>After imported, the textures will have their alpha transparency off and will need to be turned on manually. <br>A fix for this will be applied soon.

<p align="center">
 <img src="https://i.imgur.com/r71gO0i.png">
</p>

## Features

Currently, Folder Icons primary features are:

### Folder Custom Textures/Icons

Project Folders can have custom icons and textures applied through the Folder Icons ScriptableObject.
You can add and remove icons from the list displayed at the bottom of the settings.

<p align="center">
 <img src="https://user-images.githubusercontent.com/73669610/192280809-39e4f033-6e59-41eb-be85-3eab4ae1a4c1.PNG">
</p>

The icons themselves can be modified right above, in the icon editor. Simply select the icon to modify in the list and change its properties. It is possible to customize the folder textures as well as the icon to overlay on top of it. The folder texture can optionally be tinted with a color.

The overlay icon scale and position can be tweaked  at the top of the settings, both for small and large folder independently.

<p align="center">
 <img src="https://user-images.githubusercontent.com/73669610/192281030-e5de430a-ea90-42c7-b721-d5b8722408a8.PNG">
</p>

At last, it is possible to customize a gradient that will be display along the folder in the project tree view.

<p align="center">
 <img src="https://user-images.githubusercontent.com/73669610/192281467-fe5ce3d5-079d-4cde-8354-01123a46be73.png">
</p>

### Flat Texture Creator

Generally, you don't want folders to be noisey or overly messy. Directories need to be clean and appealing to look at. 
<br><br>
The texture creator, will take in a texture, and apply a custom colour to all visible pixels in respect to the alpha. These textures can then be saved and used for folders or icons. 

<p align="center">
 <img src="https://user-images.githubusercontent.com/73669610/192281153-fbb80688-eb90-4008-9feb-43263ecce30e.PNG">
</p>

## Support

If you ever come across any issues please feel free to report to the [Issues](https://github.com/WooshiiDev/Unity-Folder-Icons/issues) page on this repository. All feedback is appreciated, and will be
taken on board for any future updates. 

## License

MIT License 

https://github.com/WooshiiDev/Unity-Folder-Icons/blob/main/LICENSE
