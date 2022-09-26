using System.IO;
using UnityEngine;
using UnityEditor;

namespace FolderIcons
{
    public class CreateTextureWindow : EditorWindow
    {
        // TODO : Try and Update to the v0.2 version

        // Texture
        private Texture2D selectedTexture = null;
        private Texture2D previewTexture = null;

        private GUIContent previewContent = new();
        private RenderTexture previewRender;

        private Color replacementColour = Color.gray;

        // Texture Save Settings
        private string textureName = "New Texture";
        private string savePath;

        // Styling
        private GUIStyle previewStyle;

        private void OnEnable()
        {
            previewStyle ??= new(EditorStyles.label)
                {
                    fixedHeight = 64,
                    alignment = TextAnchor.MiddleCenter
                };

        }

        public static void ShowWindow(Texture2D folderTexture, Optional<Color> colorTint)
        {
            CreateTextureWindow window = EditorWindow.GetWindow<CreateTextureWindow>(true, "Icon Texture Creator");
            window.Initialize(folderTexture, colorTint);
            window.Show();
        }

        public void Initialize(Texture2D folderTexture, Optional<Color> colorTint)
        {
            selectedTexture = folderTexture;
            if (colorTint)
                replacementColour = colorTint;

            if (selectedTexture != null)
            {
                UpdatePreview();
            }
        }

        void OnGUI()
        {
            DrawTexturePreview();

            EditorGUI.BeginDisabledGroup(previewTexture == null);
            {
                EditorGUI.BeginChangeCheck();
                {
                    replacementColour = EditorGUILayout.ColorField(new GUIContent("Replacement Colour"), replacementColour);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    SetPreviewColour();
                }

                DrawTextureSaving();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void DrawTexturePreview()
        {
            EditorGUILayout.LabelField("Texture Colour Replacement", EditorStyles.boldLabel);

            // Draw selection
            EditorGUI.BeginChangeCheck();
            {
                // Headers
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Original Texture");
                EditorGUILayout.LabelField("Modified Texture");
                EditorGUILayout.EndHorizontal();

                // Texture -- Preview
                EditorGUILayout.BeginHorizontal();
                {
                    selectedTexture = EditorGUILayout.ObjectField(selectedTexture, typeof(Texture2D),
                        false, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.Width(64f)) as Texture2D;

                    EditorGUILayout.LabelField(previewContent, previewStyle, GUILayout.Height(64));
                }
                EditorGUILayout.EndHorizontal();
            }
            if (EditorGUI.EndChangeCheck())
            {
                if (selectedTexture == null)
                {
                    ClearPreviewData();
                    previewTexture = null;
                    return;
                }

                UpdatePreview();
            }

            EditorGUILayout.Space();
        }

        /// <summary>
        /// Display settings for saving the modified texture
        /// </summary>
        private void DrawTextureSaving()
        {
            EditorGUILayout.LabelField("Save Created Texture", EditorStyles.boldLabel);

            // Texture Name
            GUILayout.BeginHorizontal();
            {
                textureName = EditorGUILayout.TextField("Texture Name", textureName);

                EditorGUI.BeginDisabledGroup(true);
                GUILayout.TextField(".png", GUILayout.Width(40f));
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndHorizontal();

            // Save Path
            GUILayout.BeginHorizontal();
            {
                savePath = EditorGUILayout.TextField("Save Path", savePath);

                if (GUILayout.Button("Select", GUILayout.MaxWidth(80f)))
                {
                    savePath = EditorUtility.OpenFolderPanel("Texture Save Path", "Assets", "");
                    GUIUtility.ExitGUI();
                }
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Save Texture"))
            {
                string fullPath = $"{savePath}/{textureName}.png";
                SaveTextureAsPNG(previewTexture, fullPath);
            }
        }

        /// <summary>
        /// Apply colour to preview texture
        /// </summary>
        private void SetPreviewColour()
        {
            for (int x = 0; x < previewTexture.width; x++)
            {
                for (int y = 0; y < previewTexture.height; y++)
                {
                    Color oldCol = previewTexture.GetPixel(x, y);
                    Color newCol = replacementColour;
                    newCol.a = oldCol.a;

                    previewTexture.SetPixel(x, y, newCol);
                }
            }

            previewTexture.Apply();
        }

        /// <summary>
        /// Save the preview texture at the given path
        /// </summary>
        /// <param name="texture">The preview texture</param>
        /// <param name="path">The path to save the texture at</param>
        private void SaveTextureAsPNG(Texture2D texture, string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !path.Contains("Assets"))
            {
                Debug.LogWarning("Cannot save texture to invalid path.");
                return;
            }

            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);

            AssetDatabase.Refresh();

            int localPathIndex = path.IndexOf("Assets");
            path = path[localPathIndex..];

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            importer.textureType = TextureImporterType.GUI;
            importer.isReadable = true;

            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();

            //Texture2D textureAsset = AssetDatabase.LoadAssetAtPath<Texture2D> (path);
            //textureAsset.alphaIsTransparency = true;
            //textureAsset.Apply ();
        }

        /// <summary>
        /// Clear preview render texture
        /// </summary>
        private void ClearPreviewData()
        {
            if (previewRender != null)
            {
                previewRender.Release();
            }
        }

        /// <summary>
        /// Update the preview with new texture information
        /// </summary>
        private void UpdatePreview()
        {
            ClearPreviewData();

            //No real point having a huge texture so limit the size for efficency sake
            int width = Mathf.Min(256, selectedTexture.width);
            int height = Mathf.Min(256, selectedTexture.height);

            //Create a new render texture and preview
            previewRender = new RenderTexture(width, height, 16);
            previewTexture = new Texture2D(previewRender.width, previewRender.height)
            {
                alphaIsTransparency = true
            };
            previewContent.image = previewTexture;

            Graphics.Blit(selectedTexture, previewRender);

            // Get pixels from current render texture and apply
            previewTexture.ReadPixels(new Rect(0, 0, previewRender.width, previewRender.height), 0, 0);
            SetPreviewColour();
        }
    }
}