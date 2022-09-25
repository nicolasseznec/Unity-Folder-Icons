using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;


namespace FolderIcons
{
    [CustomEditor(typeof(FolderIconSettings))]
    public class FolderIconSettingsEditor : Editor
    {
        // References
        private FolderIconSettings settings;
        private SerializedProperty serializedIcons;
        private SerializedProperty serializedIconEditor;

        // Editor
        private readonly List<SerializedProperty> propertiesToDraw = new();
        private ReorderableList iconList;
        private Vector2 scrollPos;

        //Texture
        private Texture2D defaultFolder;
        private Texture2D defaultFolderOpen;

        private const float SCROLLVIEW_HEIGHT = 250f;
        private const float PROPERTY_HEIGHT = 19f;

        #region Unity Callbacks
        private void OnEnable()
        {
            if (target == null) return;

            settings = target as FolderIconSettings;
            serializedIcons = serializedObject.FindProperty(nameof(FolderIconSettings.icons));

            iconList ??= new ReorderableList(serializedObject, serializedIcons, true, displayHeader: false, true, true)
            {
                drawElementCallback = OnElementDraw,
                onSelectCallback = OnSelected,
                onAddCallback = OnAdd,
                onRemoveCallback = OnRemove,
            };

            AddCustomProperty(nameof(FolderIconSettings.showOverlay));
            AddCustomProperty(nameof(FolderIconSettings.showCustomFolder));
            AddCustomProperty(nameof(FolderIconSettings.largeIconProperties));
            AddCustomProperty(nameof(FolderIconSettings.smallIconProperties));

            serializedIconEditor = serializedObject.FindProperty(nameof(FolderIconSettings.iconEditor));

            defaultFolder = AssetUtility.GetTexture(FolderIconConstants.TEX_FOLDER_CLOSED);
            defaultFolderOpen = AssetUtility.GetTexture(FolderIconConstants.TEX_FOLDER_OPEN);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

            DrawCustomProperties();
            EditorGUILayout.Separator();

            DrawIconEditor();
            EditorGUILayout.Separator();

            DrawIconList();

            serializedObject.ApplyModifiedProperties();
        }
        #endregion

        #region Reordarable Icons list
        private void DrawIconList()
        {
            Rect headerRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(PROPERTY_HEIGHT));
            DrawCustomListHeader(headerRect);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(SCROLLVIEW_HEIGHT));
            iconList.DoLayoutList();
            EditorGUILayout.EndScrollView();
        }

        private void DrawCustomListHeader(Rect rect)
        {
            Handles.BeginGUI();
            Handles.DrawSolidRectangleWithOutline(rect,
                new Color(0.15f, 0.15f, 0.15f, 1f),
                new Color(0.15f, 0.15f, 0.15f, 1f));
            Handles.EndGUI();

            rect.x += 6f;

            EditorGUI.LabelField(rect, "Icons", EditorStyles.boldLabel);
        }

        private void OnElementDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = serializedIcons.GetArrayElementAtIndex(index);

            if (element.managedReferenceValue == null)
            {
                EditorGUI.LabelField(rect, "<null>");
                return;
            }

            // Draw Icon Preview
            float fullWidth = rect.width;
            rect.width = rect.height;
            DrawListIconPreview(rect, element);

            // Draw Icon Name
            var name = element.FindPropertyRelative(nameof(FolderIconSettings.FolderData.name));

            rect.x += rect.width + 5f;
            rect.width = fullWidth - rect.width;
            EditorGUI.LabelField(rect, name.stringValue);
        }

        private void DrawListIconPreview(Rect rect, SerializedProperty property)
        {
            FolderGUI.DrawFolderPreviewFromProperty(rect, property, open: false, small: true);
        }

        private void OnSelected(ReorderableList list)
        {
            int selected = list.index;
            var iconProp = (selected < serializedIcons.arraySize) ? serializedIcons.GetArrayElementAtIndex(selected) : null;
            UpdateIconEditor(iconProp);
        }

        private void OnRemove(ReorderableList list)
        {
            // Remove every GUID of the removed icon from the iconMap 
            int removed = list.index;
            var iconProp = serializedIcons.GetArrayElementAtIndex(removed);
            var foldersProp = iconProp.FindPropertyRelative(nameof(FolderIconSettings.FolderData.folders));
            foreach (SerializedProperty folderProp in foldersProp)
            {
                var guidProp = folderProp.FindPropertyRelative(nameof(FolderIconSettings.FolderData.FolderRef.guid));
                string guid = guidProp.stringValue;
                if (!string.IsNullOrEmpty(guid) && settings.HasGUID(guid))
                {
                    settings.iconMap.Remove(guid);
                }
            }

            ReorderableList.defaultBehaviours.DoRemoveButton(list);
            OnSelected(list);
        }

        private void OnAdd(ReorderableList list)
        {
            ReorderableList.defaultBehaviours.DoAddButton(list);

            // Create the element
            var newIcon = new FolderIconSettings.FolderData
            {
                name = "New Icon",
                colorTint = new Optional<Color>(FolderIconConstants.DefaultFolderColor),
                folderIcon = defaultFolder,
                folderIconOpen = defaultFolderOpen,
            };
            var iconProp = serializedIcons.GetArrayElementAtIndex(list.index);
            iconProp.managedReferenceValue = newIcon;

            UpdateIconEditor(iconProp);

            scrollPos.y += 30; // Ensure footer is still visible
        }
        #endregion

        #region Icon Editor
        private void DrawIconEditor()
        {
            if (serializedIconEditor.managedReferenceValue != null)
            {
                FolderDataPropertyDrawer.folderDataIndex = iconList.index;
                EditorGUILayout.PropertyField(serializedIconEditor);
                return;
            }
            else
            {
                DrawEmptyIconEditor();
            }
        }

        private void UpdateIconEditor(SerializedProperty property)
        {
            if (property != null)
                serializedIconEditor.managedReferenceId = property.managedReferenceId;
            else
                serializedIconEditor.managedReferenceValue = null;
        }

        private void DrawEmptyIconEditor()
        {
            // TODO : Box with button to add element to the icon list, and disclaimer that reads "No Icon has been created yet."
            //float height = EditorGUI.GetPropertyHeight(serializedIconEditor);
            //Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(height));

            //Rect rect = EditorGUILayout.BeginVertical();
            //EditorGUILayout.HelpBox("No Icon has been created yet.", MessageType.None, false);
            //if (GUILayout.Button("Create New Icon", GUILayout.Width(200)))
            //{
            //    OnAdd(iconList);
            //}
        }
        #endregion

        #region General Custom Properties
        private SerializedProperty AddCustomProperty(string propertyName)
        {
            var property = serializedObject.FindProperty(propertyName);
            propertiesToDraw.Add(property);
            return property;
        }

        protected void DrawCustomProperties()
        {
            foreach (var property in propertiesToDraw)
            {
                EditorGUILayout.PropertyField(property);
            }
        }
        #endregion
    }
}