using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using PhotoshopFile;

namespace Psd2uGui.Editor
{
    class ConvertEditor : EditorWindow
    {
        private Texture2D originTexture;
        private PsdFile originPsd;
        private Font fontData;

        private Vector2 originSize
        {
            get
            {
                if (originPsd == null)
                {
                    return Vector2.zero;
                }
                return originPsd.BaseLayer.Rect.size;
            }
        }

        SaveAssetEditor saveEditor;
        ConvertParameter parameter;

        [MenuItem("Window/UI/Psd2uGui Converter")]
        static void Open()
        {
            ShowWidnow(null);
        }

        [MenuItem("Assets/Convert to uGUI", true, 20000)]
        private static bool ConvertEnabled()
        {
            if (Selection.objects.Length != 1)
            {
                return false;
            }
            return AssetDatabase.GetAssetPath(Selection.objects[0]).EndsWith(".psd", StringComparison.CurrentCultureIgnoreCase);
        }

        [MenuItem("Assets/Convert to uGUI", false, 20000)]
        private static void OpenConvertEditor()
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GetAssetPath(Selection.objects[0]));
            ShowWidnow(tex);
        }

        private static void ShowWidnow(Texture2D origin)
        {
            var win = GetWindow<ConvertEditor>("Psd2uGui");
            win.originTexture = origin;
            win.originPsd = null;

            win.Show();
        }

        private void OnEnable()
        {
            var path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)).Split('/').ToList();
            path.RemoveRange(path.Count - 2, 2);

            parameter = AssetDatabase.LoadAssetAtPath<ConvertParameter>(string.Join("/", path.ToArray()) + "/ConvertParameter.asset");
            if (parameter == null)
            {
                Debug.LogError("not found convert parameter asset.");
            }
            fontData = parameter.defaultFont;

            GameObject canvasObj = GameObject.Find("Canvas");
            if (canvasObj == null)
            {
                canvasObj = new GameObject("Canvas");
                var canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            originTexture = (Texture2D)EditorGUILayout.ObjectField("psd", originTexture, typeof(Texture2D), false);
            if (EditorGUI.EndChangeCheck() || (originTexture != null && originPsd == null))
            {
                UpdateOriginTexture();
            }

            fontData = (Font)EditorGUILayout.ObjectField("font", fontData, typeof(Font), false);
            EditorGUILayout.Space();

            if(saveEditor != null)
            {
                saveEditor.Draw();
            }

            if (GUILayout.Button("Convert"))
            {
                Convert();
                UpdateOriginTexture();
            }
        }


        private void UpdateOriginTexture()
        {
            if (!AssetDatabase.GetAssetPath(originTexture).EndsWith("psd"))
            {
                originTexture = null;
                Debug.LogError("it is not psd file.");
                return;
            }
            var path = Application.dataPath.Split('/');
            path[path.Length - 1] = AssetDatabase.GetAssetPath(originTexture);
            originPsd = new PsdFile(string.Join("/", path), new LoadContext { Encoding = Encoding.Default });

            if(saveEditor != null)
            {
                saveEditor.Dispose();
            }
            saveEditor = new SaveAssetEditor(originPsd, parameter.textureSavePath);
        }

        private void Convert()
        {
            saveEditor.Save();

            var layers = EditorUtil.GetHierarchyPath(originPsd);
            layers.Where(x =>
            {
                if(x.Key.AdditionalInfo.Any(info => info is LayerSectionInfo))
                {
                    return false;
                }
                if(x.Key.Rect.width <= 0 || x.Key.Rect.height <= 0)
                {
                    return false;
                }
                return x.Key.Visible;
            }).ToDictionary(k => k, v => v);

            var sprites = layers.Where(x => !x.Key.AdditionalInfo.Any(info => info is TextLayerInfo)).Select(x => x.Key.Name).Distinct().Select(x =>
            {
                return AssetDatabase.LoadAssetAtPath<Sprite>(EditorUtil.GetAssetPath(parameter.textureSavePath, x));
            }).Where(x => x != null).ToArray();

            CreateGUI(layers, sprites);
        }

        private void CreateGUI(Dictionary<Layer, string> layers, Sprite[] sprites)
        {
            GameObject canvasObj = GameObject.Find("Canvas");

            if (canvasObj == null)
            {
                Debug.LogError("not found canvas object.");
                return;
            }

            var root = new GameObject(originTexture.name, typeof(RectTransform));
            root.transform.SetParent(canvasObj.transform, false);
            root.GetComponent<RectTransform>().sizeDelta = originSize;

            var components = new Converter(parameter, sprites, fontData).ConvertLayerComponents(layers);

            foreach (var component in components)
            {
                var posX = component.Rect.center.x - originSize.x / 2f;
                var posY = (originSize.y / 2f) - component.Rect.center.y;
                var rect = GetOrCreateTransform(root.transform, component.Path, component.Name, new Vector2(posX, posY));

                component.Create(rect);
            }
        }

        private RectTransform GetOrCreateTransform(Transform parent, string hierarchyPath, string name, Vector2 position)
        {
            Transform root = parent;
            var path = hierarchyPath.Split('/');

            for (int i = 0; i < path.Length; i++)
            {
                var t = root.Find(path[i]);
                if (t == null)
                {
                    t = CreateTransform(root, path[i]);
                }
                root = t;
            }

            var obj = root.Find(name);
            if (obj == null || obj.position != position)
            {
                obj = CreateTransform(root, name);
                obj.position = position;
            }

            return obj.GetComponent<RectTransform>();
        }

        private Transform CreateTransform(Transform parent, string name)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one / 2f;
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.sizeDelta = Vector2.zero;

            return obj.transform;
        }

    }
}
