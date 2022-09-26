using UnityEditor;
using UnityEngine;

namespace FolderIcons
{
    /// <summary>
    /// Contains all constant data and settings required.
    /// </summary>
    internal static class FolderIconConstants
    {
        #region Settings
        // Settings

        /// <summary>
        /// The default path for creating <see cref="FolderIconSettings"/>.
        /// </summary>
        public const string ASSET_DEFAULT_PATH = "Assets/";

        /// <summary>
        /// The EditorPref Key for saving the last known GUID of a <see cref="FolderIconSettings"/> instance.
        /// </summary>
        public const string PREF_GUID = "FP_GUID";

        /// <summary>
        /// The EditorPref Key for showing custom folder textures.
        /// </summary>
        public const string PREF_FOLDER = "FP_SHOW_FOLDERS";

        /// <summary>
        /// The EditorPref Key for showing custom folder icons.
        /// </summary>
        public const string PREF_OVERLAY = "FP_SHOW_ICONS";
        #endregion

        #region GUI
        // GUI

        /// <summary>
        /// The maximum height of a texture in the project treeview.
        /// </summary>
        public const float MAX_TREE_HEIGHT = 16f;

        /// <summary>
        /// The maximum height of a texture in the standard project view.
        /// </summary>
        public const float MAX_PROJECT_HEIGHT = 110f;
        #endregion

        #region Colors
        // Colours
        public static readonly Color BgFaceColor = new(0.18f, 0.18f, 0.18f, 1f);
        
        public static readonly Color BgOutlineColor = new(0.15f, 0.15f, 0.15f, 1f);

        private static readonly bool isDarkTheme = EditorGUIUtility.isProSkin;

        public static readonly Color SelectedColor = new Color32(44, 93, 135, 255);

        public static readonly Color UnfocusColor = new Color32(77, 77, 77, 255);

        public static readonly Color DefaultFolderColor = new(0.76f, 0.76f, 0.76f); // TODO : light theme

        /// <summary>
        /// The color of the background in the main project view.
        /// </summary>
        public static readonly Color SkinDefaultColor = isDarkTheme
          ? new Color32(51, 51, 51, 255)
          : new Color32(190, 190, 190, 255);

        /// <summary>
        /// The color of the background in the tree project view.
        /// </summary>
        public static readonly Color SkinTreeViewColor = isDarkTheme
          ? new Color32(56, 56, 56, 255)
          : new Color32(190, 190, 190, 255); // TODO : light theme
        #endregion

        #region Textures
        // Textures

        public const string TEX_FOLDER_CLOSED = "Folder_Default_Closed";
        public const string TEX_FOLDER_OPEN = "Folder_Default_Open";
        #endregion 
    }
}