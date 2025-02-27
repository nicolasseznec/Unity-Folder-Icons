﻿using UnityEditor;
using UnityEngine;


namespace FolderIcons
{
    [InitializeOnLoad]
    internal static class FolderIcons
    {
        private const string SETTINGS_TYPE_STRING = "FolderIconSettings";
        private const string SETTINGS_NAME_STRING = "Folder Icons";

        public static FolderIconSettings settings;

        static FolderIcons()
        {
            ProjectViewUtility.Initialize();

            // Find scriptable instance or create one
            FetchSettings();

            // Setup callback
            EditorApplication.projectWindowItemOnGUI -= OnFolderGUI;
            EditorApplication.projectWindowItemOnGUI += OnFolderGUI;
        }

        private static void OnFolderGUI(string guid, Rect selectionRect)
        {
            if (Application.isPlaying || Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (settings == null)
            {
                FetchSettings();
                return;
            }

            if (!settings.showCustomFolder && !settings.showOverlay)
            {
                return;
            }

            ProjectViewUtility.UpdateOpenFolderCache(guid, selectionRect);

            DefaultAsset folderAsset = AssetUtility.LoadAssetFromGUID<DefaultAsset>(guid);

            if (folderAsset == null)
            {
                return;
            }

            if (settings.iconMap.TryGetValue(guid, out FolderIconSettings.FolderData icon))
            {
                FolderGUI.DrawCustomFolder(selectionRect, guid, folderAsset.GetInstanceID(), settings, icon);
                ProjectViewUtility.ResetIsPrevCustomIcon();
            }
        }

        private static FolderIconSettings GetOrCreateSettings()
        {
            string path = null;

            // Make sure the key is still valid - no assuming that settings just 'exist'
            string guidPref = FolderIconConstants.PREF_GUID;
            if (EditorPrefs.HasKey(guidPref))
            {
                if (AssetUtility.TryGetAsset(EditorPrefs.GetString(guidPref), out path))
                {
                    return AssetDatabase.LoadAssetAtPath<FolderIconSettings>(path);
                }
            }

            FolderIconSettings settings = AssetUtility.FindOrCreateScriptable<FolderIconSettings>(SETTINGS_TYPE_STRING, SETTINGS_NAME_STRING, FolderIconConstants.ASSET_DEFAULT_PATH);

            path = AssetDatabase.GetAssetPath(settings);
            EditorPrefs.SetString(guidPref, AssetDatabase.AssetPathToGUID(path));

            return settings;
        }

        private static void FetchSettings()
        {
            settings = GetOrCreateSettings();
            settings.OnInitalize();
        }
    }
}