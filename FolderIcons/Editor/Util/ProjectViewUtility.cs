using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;


namespace FolderIcons
{
    /// <summary>
    /// Methods to get information about the project view window.
    /// </summary>
	public static class ProjectViewUtility
    {
        // Reflection (for tree view focus detection)
        private static Assembly editorAssembly;
        private static Type projectBrowserType;
        private static Type treeViewControllerType;
        private static Type projectWindowUtilType;

        private static FieldInfo m_FolderTree;
        private static MethodInfo hasFocusMethod;
        private static MethodInfo getActiveFolderPath;

        // Knowing if a folder is open in tree view
        private static string prevGuid;
        private static bool isPrevCustomIcon;
        private static Rect prevRect;
        private static Dictionary<string, bool> openFoldersCache;

        public static void Initialize()
        {
            // Reflection
            editorAssembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));

            projectBrowserType = editorAssembly.GetType("UnityEditor.ProjectBrowser");
            treeViewControllerType = editorAssembly.GetType("UnityEditor.IMGUI.Controls.TreeViewController");

            m_FolderTree = projectBrowserType.GetField("m_FolderTree", BindingFlags.NonPublic | BindingFlags.Instance);
            hasFocusMethod = treeViewControllerType.GetMethod("HasFocus", BindingFlags.Instance | BindingFlags.Public);

            projectWindowUtilType = typeof(ProjectWindowUtil);
            getActiveFolderPath = projectWindowUtilType.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);

            // Open folders
            openFoldersCache = new Dictionary<string, bool>();
        }

        /// <summary>
        /// Determines whether the tree view part of the project window has focus.
        /// </summary>
        public static bool HasTreeViewFocus()
        {
            var focusedWindow = EditorWindow.focusedWindow;
            //if (!focusedWindow.ToString().Contains("UnityEditor.ProjectBrowser"))
            if (focusedWindow.ToString() != " (UnityEditor.ProjectBrowser)")
            {
                return false;
            }

            var folderTree = m_FolderTree.GetValue(focusedWindow);
            var hasFocus = hasFocusMethod.Invoke(folderTree, null);

            return (bool)hasFocus;
        }

        public static void ResetIsPrevCustomIcon()
        {
            isPrevCustomIcon = true;
        }

        public static void UpdateOpenFolderCache(string currentGUID, Rect currentRect)
        {
            // Hacky solution to know which folders are open(expanded) in the tree view, based on the fact 
            // items are drawn from top to bottom, and open folders have their children indented.

            // Another way could be to use InternalEditorUtility.expandedProjectWindowItems, but I do not know 
            // how efficient it would be.

            if (IsTreeView(prevRect) && IsTreeView(currentRect) && isPrevCustomIcon)
            {
                openFoldersCache[prevGuid] = prevRect.x < currentRect.x; // previous folder is open if the current one is indented compared to it.
            }

            prevRect = currentRect;
            isPrevCustomIcon = false;
            prevGuid = currentGUID;
        }

        /// <summary>
        /// Determines if a folder is expanded in the project tree view.
        /// </summary>
        /// <param name="guid">The GUID of the folder</param>
        public static bool IsFolderOpen(string guid)
        {
            return openFoldersCache.ContainsKey(guid) && openFoldersCache[guid];
        }

        /// <summary>
        /// Determines if a folder is selected in the project tree view.
        /// </summary>
        /// <param name="path">The path to the folder</param>
        public static bool IsFolderSelected(string path)
        {
            object obj = getActiveFolderPath.Invoke(null, new object[0]);
            string pathToSelected = obj.ToString();

            return path == pathToSelected;
        }

        /// <summary>
        /// Determines if the given rectangle concerns an item in the project tree view.
        /// </summary>
        private static bool IsTreeView(Rect rect)
        {
            return FolderGUI.IsTreeView(rect) && FolderGUI.IsSideView(rect);
        }
    }
}