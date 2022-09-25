using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using FolderData = FolderIcons.FolderIconSettings.FolderData;

namespace FolderIcons
{
    [CustomPropertyDrawer(typeof(FolderData))]
    public class FolderDataPropertyDrawer : PropertyDrawer
    {
        private const float MAX_LABEL_WIDTH = 100f;
        private const float MAX_FIELD_WIDTH = 150f;

        private const float PROPERTY_HEIGHT = 19f;
        private const float PROPERTY_PADDING = 4f;
        private const float PROPERTY_INDENT = 10f;

        private const float SCROLLVIEW_HEIGHT = 122f;

        private readonly Dictionary<string, ReorderableList> reorderableLists = new();

        private Vector2 scrollPos;
        private Rect listRect;
        private Rect scrollViewRect;

        private ReorderableList folders;
        private SerializedProperty foldersProp;
        private SerializedProperty iconProp;

        // Serves to know which icon is being modified. Could also look into the serialized property path instead.
        public static int folderDataIndex;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            UpdateListCache(property);
            //float listHeight = _reorderableLists[property.propertyPath].GetHeight();

            return (PROPERTY_HEIGHT + PROPERTY_PADDING) * 19 + PROPERTY_PADDING * 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            iconProp = property;

            position.y += PROPERTY_PADDING;

            DrawBackground(position);
            position = DrawHeader(position);

            Rect originalRect = position;
            position.width = Mathf.Min(position.width, MAX_LABEL_WIDTH + MAX_FIELD_WIDTH);

            Rect sidePropertiesRect = DrawSideProperties(position, property);
            float lastPropertyY = sidePropertiesRect.yMax;

            position.x += position.width;
            position.width = originalRect.width - position.width;

            DrawPreview(position, property, sidePropertiesRect.height);

            position.width = originalRect.width;
            position.x = originalRect.x + PROPERTY_INDENT;
            position.y = lastPropertyY + PROPERTY_PADDING * 2;

            position.y += 73f; // TODO : proper scaling

            DrawFolders(position, property);

            EditorGUI.EndProperty();
        }

        private void DrawPreview(Rect position, SerializedProperty property, float sidePropertiesHeight)
        {
            // Draw Large Preview
            position.height = sidePropertiesHeight;
            //position.height = position.width;
            FolderGUI.DrawFolderPreviewFromProperty(position, property, open: false, small: false);

            // Draw Small Closed Preview
            float fullWidth = Mathf.Min(position.width, sidePropertiesHeight);
            float middle = position.x + position.width * 0.5f;

            position.y += position.height;
            position.height *= 0.4f;

            position.x = middle - fullWidth * 0.4f;
            position.width = fullWidth * 0.3f;
            FolderGUI.DrawFolderPreviewFromProperty(position, property, open: false, small: true);

            // Draw Small Open Preview
            position.x = middle + fullWidth * 0.1f;
            FolderGUI.DrawFolderPreviewFromProperty(position, property, open: true, small: true);
        }

        private Rect DrawHeader(Rect rect)
        {
            Rect _rect = rect;
            _rect.height = PROPERTY_HEIGHT;

            Handles.BeginGUI();
            Handles.DrawSolidRectangleWithOutline(_rect,
                new Color(0.20f, 0.20f, 0.20f, 1f),
                new Color(0.15f, 0.15f, 0.15f, 1f));
            Handles.EndGUI();
            _rect.x += 6f;
            EditorGUI.LabelField(_rect, "Icon Editor", EditorStyles.boldLabel);

            rect.y += PROPERTY_HEIGHT;
            return rect;
        }

        private void DrawBackground(Rect rect)
        {
            rect.width += 4f;
            rect.x -= 2f;

            Handles.BeginGUI();
            Handles.DrawSolidRectangleWithOutline(rect, Color.clear, new Color(0.15f, 0.15f, 0.15f, 1f));
            Handles.EndGUI();
        }

        #region Draw Side Properties
        private Rect DrawSideProperties(Rect rect, SerializedProperty property)
        {
            Rect sidePropertiesRect = rect;

            float originalLabelWidth = EditorGUIUtility.labelWidth;
            float shortLabelWidth = Mathf.Min(EditorGUIUtility.labelWidth, MAX_LABEL_WIDTH);
            EditorGUIUtility.labelWidth = shortLabelWidth;

            rect.x++;
            rect.width -= 2;
            rect.y += PROPERTY_PADDING;
            rect.height = PROPERTY_HEIGHT;

            SerializedProperty copy = property.Copy();
            bool enterChildren = true;

            while (copy.NextVisible(enterChildren))
            {
                if (SerializedProperty.EqualContents(copy, property.GetEndProperty()))
                    break;

                switch (copy.name)
                {
                    case nameof(FolderData.name):
                        DrawName(rect, copy);
                        rect.y += PROPERTY_HEIGHT + PROPERTY_PADDING;
                        break;

                    case nameof(FolderData.treeGradient):
                        DrawGradient(rect, copy, property);
                        rect.y += PROPERTY_HEIGHT + PROPERTY_PADDING;
                        break;

                    case nameof(FolderData.coverBackground):
                        DrawCoverBackground(rect, copy, shortLabelWidth);
                        rect.y += PROPERTY_HEIGHT + PROPERTY_PADDING;
                        break;

                    case nameof(FolderData.folders):
                        break; // Not a side property (drawn later)

                    default:
                        EditorGUI.PropertyField(rect, copy, true);
                        rect.y += PROPERTY_HEIGHT + PROPERTY_PADDING;
                        break;
                }

                enterChildren = false;
            }

            EditorGUIUtility.labelWidth = originalLabelWidth;

            sidePropertiesRect.yMax = rect.yMax;

            return sidePropertiesRect;
        }

        private void DrawName(Rect rect, SerializedProperty nameProp)
        {
            rect.width = MAX_FIELD_WIDTH;
            rect.y += 2f;
            EditorGUI.PropertyField(rect, nameProp, GUIContent.none);
        }

        private void DrawGradient(Rect rect, SerializedProperty gradientProp, SerializedProperty iconProp)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(rect, gradientProp, true);
            if (EditorGUI.EndChangeCheck())
            {
                var enableProp = gradientProp.FindPropertyRelative("enabled");
                if (enableProp.boolValue)
                {
                    FolderGUI.UpdateGradientTexture(iconProp.managedReferenceValue as FolderData);
                }
            }
        }

        private void DrawCoverBackground(Rect rect, SerializedProperty coverProp, float originalLabelWidth)
        {
            EditorGUIUtility.labelWidth = MAX_LABEL_WIDTH + MAX_FIELD_WIDTH - PROPERTY_HEIGHT - 3;
            EditorGUI.PropertyField(rect, coverProp, true);
            EditorGUIUtility.labelWidth = originalLabelWidth;
        }
        #endregion

        #region Reorderable Folder List
        private void UpdateListCache(SerializedProperty property)
        {
            SerializedProperty foldersProp = property.FindPropertyRelative(nameof(FolderData.folders));

            if (!reorderableLists.ContainsKey(property.propertyPath)
                || reorderableLists[property.propertyPath].index > reorderableLists[property.propertyPath].count - 1)
            {
                reorderableLists[property.propertyPath] = new ReorderableList(foldersProp.serializedObject, foldersProp, true, displayHeader: false, true, true)
                {
                    drawElementCallback = OnElementDraw,
                    drawFooterCallback = OnFooterDraw,
                    onAddCallback = OnAdd,
                    onRemoveCallback = OnRemove,
                };
            }
        }

        private void DrawFolders(Rect rect, SerializedProperty property)
        {
            // Update references
            folders = reorderableLists[property.propertyPath];
            foldersProp = property.FindPropertyRelative(nameof(FolderData.folders));

            rect.x += 2f;
            rect.width -= 20f;

            DrawListHeader(rect);

            rect.height = SCROLLVIEW_HEIGHT;
            rect.y += PROPERTY_HEIGHT;

            Rect viewRect = rect;
            viewRect.height = folders.GetHeight();
            viewRect.width -= 20f;

            listRect = viewRect;
            scrollViewRect = rect;

            scrollPos = GUI.BeginScrollView(rect, scrollPos, viewRect);

            reorderableLists[property.propertyPath].DoList(viewRect);
        }

        private void OnElementDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            var folderRef = foldersProp.GetArrayElementAtIndex(index);
            var folder = folderRef.FindPropertyRelative(nameof(FolderData.FolderRef.folder));

            rect.height = PROPERTY_HEIGHT;
            rect.width = MAX_FIELD_WIDTH + MAX_LABEL_WIDTH;
            rect.x += MAX_LABEL_WIDTH * 0.5f;
            rect.y += 2f;

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(rect, folder, GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
                var guidProp = folderRef.FindPropertyRelative(nameof(FolderData.FolderRef.guid));
                string oldGUID = guidProp.stringValue;
                string newGUID = AssetUtility.GetGUIDFromAsset(folder.objectReferenceValue);

                if (oldGUID == newGUID)
                    return;
                else
                    FolderIcons.settings.RemoveGUID(oldGUID);

                if (FolderIcons.settings.CanUpdateGUID(newGUID))
                {
                    guidProp.stringValue = newGUID;
                    FolderIcons.settings.AddGUID(newGUID, (FolderData)iconProp.managedReferenceValue);
                }
                else
                {
                    guidProp.stringValue = null;
                    folder.objectReferenceValue = null;
                }
                EditorApplication.RepaintProjectWindow();
            }
        }

        private void DrawListHeader(Rect rect)
        {
            rect.height = PROPERTY_HEIGHT;
            rect.width -= 20f;
            Handles.BeginGUI();
            Handles.DrawSolidRectangleWithOutline(rect,
                new Color(0.20f, 0.20f, 0.20f, 1f),
                new Color(0.15f, 0.15f, 0.15f, 1f));
            Handles.EndGUI();
            rect.x += 6f;
            EditorGUI.LabelField(rect, "Folders", EditorStyles.boldLabel);
        }

        private void OnFooterDraw(Rect rect)
        {
            GUI.EndScrollView();
            if (listRect.height > scrollViewRect.height)
                rect.y = scrollViewRect.yMax;
            ReorderableList.defaultBehaviours.DrawFooter(rect, folders);
        }

        private void OnAdd(ReorderableList list)
        {
            ReorderableList.defaultBehaviours.DoAddButton(list);

            int added = list.index;
            var folderRef = foldersProp.GetArrayElementAtIndex(added);

            var guidProp = folderRef.FindPropertyRelative(nameof(FolderData.FolderRef.guid));
            guidProp.stringValue = null;

            var refProp = folderRef.FindPropertyRelative(nameof(FolderData.FolderRef.folder));
            refProp.objectReferenceValue = null;

            scrollPos.y += 30; // Ensure view scroll to the bottom
        }

        private void OnRemove(ReorderableList list)
        {
            int removed = list.index;
            var folderRef = foldersProp.GetArrayElementAtIndex(removed);
            var guidProp = folderRef.FindPropertyRelative(nameof(FolderData.FolderRef.guid));
            string guid = guidProp.stringValue;

            if (!string.IsNullOrEmpty(guid) && FolderIcons.settings.HasGUID(guid))
            {
                FolderIcons.settings.iconMap.Remove(guid);
            }
            ReorderableList.defaultBehaviours.DoRemoveButton(list);
        }
        #endregion
    }
}