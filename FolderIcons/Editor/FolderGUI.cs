using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using FolderData = FolderIcons.FolderIconSettings.FolderData;


namespace FolderIcons
{
    /// <summary>
    /// GUI Methods for Folder Icons.
    /// </summary>
    public static class FolderGUI
    {
        private static Dictionary<FolderData, Texture2D> gradientCache = new();

        #region Editor Preview
        /// <summary>
        /// Draw a preview of a folder icon from its property.
        /// </summary>
        /// <param name="rect">Rectangle to draw the icon within</param>
        /// <param name="iconProperty">Property of the icon to draw</param>
        /// <param name="open">Use the open folder icon</param>
        /// <param name="small">Use the small icon properties</param>
        public static void DrawFolderPreviewFromProperty(Rect rect, SerializedProperty iconProperty, bool open = false, bool small = false)
        {
            if (iconProperty == null) return;

            // Textures
            var folderProp = open
                ? iconProperty.FindPropertyRelative(nameof(FolderData.folderIconOpen))
                : iconProperty.FindPropertyRelative(nameof(FolderData.folderIcon));

            var overlayIconProp = iconProperty.FindPropertyRelative(nameof(FolderData.overlayIcon));

            Texture folderTexture = folderProp.objectReferenceValue as Texture;
            Texture overlayIconTexture = overlayIconProp.objectReferenceValue as Texture;

            // Modifiers
            var modifierProp = small
                ? iconProperty.serializedObject.FindProperty(nameof(FolderIconSettings.smallIconProperties))
                : iconProperty.serializedObject.FindProperty(nameof(FolderIconSettings.largeIconProperties));

            var offsetProp = modifierProp.FindPropertyRelative(nameof(FolderIconSettings.OverlayIconModifier.offset));
            var scaleProp = modifierProp.FindPropertyRelative(nameof(FolderIconSettings.OverlayIconModifier.scale));

            // Color
            var colorProp = iconProperty.FindPropertyRelative(nameof(FolderData.colorTint));
            Optional<Color> color = (colorProp.FindPropertyRelative("enabled").boolValue)
                ? new(colorProp.FindPropertyRelative("value").colorValue)
                : new();

            //DrawFolderPreview(rect, folderTexture, overlayIconTexture, scaleProp.floatValue, offsetProp.vector2Value, color);
            DrawFolderTexture(rect, folderTexture, color);
            DrawOverlayTexture(rect, overlayIconTexture, scaleProp.floatValue, offsetProp.vector2Value);
        }

        /// <summary>
        /// Draw a background in the editor
        /// </summary>
        /// <param name="rect">Rectangle to draw the background within</param>
        /// <param name="faceColor">Color to use inside the background</param>
        /// <param name="outlineColor">Color to use for the outline of the background</param>
        /// <param name="retract">Shrink the background by a certain amount</param>
        public static void DrawBackground(Rect rect, Color faceColor, Color outlineColor, int retract = 0)
        {
            rect.height -= retract * 2;
            rect.y += retract;
            rect.width -= retract * 2;
            rect.x += retract;

            Handles.BeginGUI();
            Handles.DrawSolidRectangleWithOutline(rect, faceColor, outlineColor);
            Handles.EndGUI();
        }
        #endregion

        #region Icon Draw
        /// <summary>
        /// Draw the folder and its overlay
        /// </summary>
        /// <param name="rect">The original folder rect.</param>
        /// <param name="guid">The GUID of the folder to draw.</param>
        /// <param name="settings">The global settings.</param>
        /// <param name="icon">The folder icon data.</param>
        public static void DrawCustomFolder(Rect rect, string guid, FolderIconSettings settings, FolderData icon)
        {
            // Parameters
            bool small = IsTreeView(rect) && IsSideView(rect);
            bool open = small && ProjectViewUtility.IsFolderOpen(guid);

            Texture2D folderIcon = open
                ? icon.folderIconOpen
                : icon.folderIcon;

            if (open && icon.folderIconOpen == null)
                folderIcon = icon.folderIcon;

            Texture2D overlayIcon = icon.overlayIcon;

            var iconProperties = small
                ? settings.smallIconProperties
                : settings.largeIconProperties;

            float scale = iconProperties.scale;
            Vector2 offset = iconProperties.offset;

            Rect folderRect = GetFolderRect(rect);

            // Drawing
            if (icon.coverBackground)
            {
                DrawBackgroundCover(folderRect, guid, small);
            }

            if (small && icon.treeGradient)
            {
                DrawIconGradient(rect, icon);
            }

            if (settings.showCustomFolder)
            {
                DrawFolderTexture(folderRect, folderIcon, icon.colorTint);
            }

            if (settings.showOverlay)
            {
                DrawOverlayTexture(folderRect, overlayIcon, scale, offset);
            }
        }

        /// <summary>
        /// Draw the folder texture.
        /// </summary>
        /// <param name="rect">The rect for the folder texture.</param>
        /// <param name="folder">The texture to draw.</param>
        /// <param name="colorTint">The optional Color to tint the texture with.</param>
        public static void DrawFolderTexture(Rect rect, Texture folder, Optional<Color> colorTint)
        {
            if (folder == null) return;

            if (colorTint)
            {
                GUI.DrawTexture(rect, folder, ScaleMode.ScaleToFit, true, 0, colorTint, 0, 0);
            }
            else
            {
                GUI.DrawTexture(rect, folder, ScaleMode.ScaleToFit);
            }
        }

        /// <summary>
        /// Draw the folder overlay texture, given the folder rect.
        /// </summary>
        /// <param name="rect">The original rect of the folder.</param>
        /// <param name="overlay">The overlay texture to draw.</param>
        /// <param name="offset">The offset to apply to the overlay.</param>
        /// <param name="scale">The scale to apply to the overlay.</param>
        public static void DrawOverlayTexture(Rect rect, Texture overlay, float scale, Vector2 offset)
        {
            if (overlay == null) return;

            offset *= Mathf.Min(rect.width, rect.height) * 0.4f; // Offset is relative to the rect size

            rect.size *= scale;
            rect.position += ((1 / scale) - 1) * 0.5f * rect.size;

            rect.x += offset.x;
            rect.y -= offset.y;

            GUI.DrawTexture(GetOverlayRect(rect), overlay, ScaleMode.ScaleToFit);
        }

        /// <summary>
        /// Draw a flat color before the icon, to cover anything below it.
        /// </summary>
        /// <param name="rect">The rectangle to cover</param>
        /// <param name="guid">GUID of the folder to cover</param>
        /// <param name="small">If the icon is in the project tree</param>
        public static void DrawBackgroundCover(Rect rect, string guid, bool small)
        {
            Color color;

            if (small)
            {
                if (ProjectViewUtility.IsFolderSelected(AssetDatabase.GUIDToAssetPath(guid)))
                {
                    color = ProjectViewUtility.HasTreeViewFocus() ? FolderIconConstants.SelectedColor : FolderIconConstants.UnfocusColor;
                }
                else
                {
                    color = FolderIconConstants.SkinTreeViewColor;
                }
            }
            else
            {
                color = FolderIconConstants.SkinDefaultColor;
            }

            EditorGUI.DrawRect(rect, color);
        }
        #endregion

        #region Util
        /// <summary>
        /// Check if the given rect is part of the project sidevew.
        /// </summary>
        /// <param name="rect">The rect to check.</param>
        public static bool IsSideView(Rect rect)
        {
#if UNITY_2019_3_OR_NEWER
            return rect.x != 14;
#else
                return rect.x != 13;
#endif
        }

        /// <summary>
        /// Check if the given rect is in a project treeview.
        /// </summary>
        /// <param name="rect">The rect to check.</param>
        public static bool IsTreeView(Rect rect)
        {
            return rect.width > rect.height;
        }

        private static Rect GetOverlayRect(Rect rect)
        {
            //Half size of overlay, and reposition to center
            rect.size *= 0.5f;
            rect.position += rect.size * 0.5f;

            return rect;
        }

        private static Rect GetFolderRect(Rect rect)
        {
            bool isTreeView = IsTreeView(rect);
            bool isSideView = IsSideView(rect);

            // Vertical Folder View
            if (isTreeView)
            {
                rect.width = rect.height = FolderIconConstants.MAX_TREE_HEIGHT;

                //Add small offset for correct placement
                if (!isSideView)
                {
                    rect.x += 3f;
                }
            }
            else
            {
                rect.height -= 14f;
            }

            return rect;
        }
        #endregion

        #region Gradient Handling
        private static void DrawIconGradient(Rect iconRect, FolderData icon)
        {
            iconRect.width += iconRect.x + 5;
            iconRect.x = 0;

            Texture2D gradTex = GetGradientTexture(icon);
            GUI.DrawTexture(iconRect, gradTex, ScaleMode.StretchToFill);
        }

        private static Texture2D GetGradientTexture(FolderData icon)
        {
            if (gradientCache.TryGetValue(icon, out Texture2D gradTex) && gradTex != null)
            {
                return gradTex;
            }
            else
            {
                return UpdateGradientTexture(icon);
            }
        }

        public static Texture2D UpdateGradientTexture(FolderData icon)
        {
            if (icon == null) return null;

            return gradientCache[icon] = GradientToTexture(icon.treeGradient);
        }

        private static Texture2D GradientToTexture(Gradient gradient, int width = 32)
        {
            Texture2D gradTex = new(width, 1)
            {
                filterMode = FilterMode.Bilinear
            };
            float step = 1f / width;
            for (int x = 0; x < width; x++)
            {
                float t = x * step;
                Color col = gradient.Evaluate(t);
                gradTex.SetPixel(x, 1, col);
            }
            gradTex.Apply();
            return gradTex;
        }
        #endregion
    }
}