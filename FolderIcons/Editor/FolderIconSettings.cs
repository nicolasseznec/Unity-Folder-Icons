using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FolderIcons
{
    [CreateAssetMenu(fileName = "Folder Icon Manager", menuName = "Scriptables/Others/Folder Manager")]
    public class FolderIconSettings : ScriptableObject
    {
        [Serializable]
        public class FolderData
        {
            public string name;

            [Tooltip("Icon image of the closed folder")]
            public Texture2D folderIcon;

            [Tooltip("Icon image of the open folder")]
            public Texture2D folderIconOpen;

            [Tooltip("Icon image to overlay on top of the folder")]
            public Texture2D overlayIcon;

            [Tooltip("Color to apply to the folder icon")]
            public Optional<Color> colorTint;

            [Tooltip("Gradient to display in the project tree view")]
            public Optional<Gradient> treeGradient;

            [Tooltip("Apply the editor background color before drawing the icon to ensure nothing below is visible")]
            public bool coverBackground;

            [Serializable]
            public class FolderRef
            {
                public string guid;
                public DefaultAsset folder;
            }

            public FolderRef[] folders;
        }

        //Global Settings
        public bool showOverlay = true;
        public bool showCustomFolder = true;

        [Serializable]
        public class OverlayIconModifier
        {
            [VectorRange(-1, 1, -1, 1)] public Vector2 offset;
            [Range(0.5f, 2f)] public float scale = 1f;
        }

        [Tooltip("Scale and offset to apply to the overlay icon on large folders")]
        public OverlayIconModifier largeIconProperties;

        [Tooltip("Scale and offset to apply to the overlay icon on small folders")]
        public OverlayIconModifier smallIconProperties;

        [SerializeReference] public FolderData iconEditor;
        [SerializeReference] public FolderData[] icons;

        public Dictionary<string, FolderData> iconMap;

        public void OnInitalize()
        {
            RecreateGUIDMap();
        }

        public void RecreateGUIDMap()
        {
            iconMap = new Dictionary<string, FolderData>();

            for (int i = 0; i < icons.Length; i++)
            {
                var folders = icons[i].folders;

                for (int j = 0; j < folders.Length; j++)
                {
                    string guid = folders[j].guid;

                    if (string.IsNullOrEmpty(guid))
                    {
                        guid = AssetUtility.GetGUIDFromAsset(folders[j].folder);
                        folders[j].guid = guid;
                    }

                    if (string.IsNullOrEmpty(guid) || iconMap.ContainsKey(guid))
                    {
                        continue;
                    }

                    iconMap.Add(guid, icons[i]);
                }
            }
        }

        public bool CanUpdateGUID(string newGUID)
        {
            return !string.IsNullOrEmpty(newGUID) && !HasGUID(newGUID);  // Forbid change when the folder is already in another icon.
        }

        public bool HasGUID(string guid)
        {
            return iconMap.ContainsKey(guid);
        }

        public void RemoveGUID(string guid)
        {
            if (!string.IsNullOrEmpty(guid) && HasGUID(guid))
                iconMap.Remove(guid);
        }

        public void AddGUID(string guid, FolderData icon)
        {
            iconMap[guid] = icon;
        }
    }
}