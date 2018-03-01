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
    internal class ConvertEditor : EditorWindow
    {
        public Texture2D originTexture;
        private Vector2 originSize;

        Dictionary<Layer, string> visibleLayers = new Dictionary<Layer, string>();

        private Vector2 scrollPosition;

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
            return AssetDatabase.GetAssetPath(Selection.objects[0]).EndsWith(".psd",  StringComparison.CurrentCultureIgnoreCase);
        }

        [MenuItem("Assets/Convert to uGUI", false, 20000)]
        private static void Convert()
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GetAssetPath(Selection.objects[0]));
            ShowWidnow(tex);
        }

        private static void ShowWidnow(Texture2D origin)
        {
            var win = GetWindow<ConvertEditor>("Psd2uGui");
            win.originTexture = origin;

            win.Show();
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            originTexture = (Texture2D)EditorGUILayout.ObjectField("psd", originTexture, typeof(Texture2D), true);
            if (EditorGUI.EndChangeCheck())
            {
                var path = Application.dataPath.Split('/');
                path[path.Length - 1] = AssetDatabase.GetAssetPath(originTexture);
                var psd = new PsdFile(string.Join("/", path), new LoadContext { Encoding = Encoding.Default });

                originSize = psd.BaseLayer.Rect.size;
                visibleLayers = GetHierarchyPath(psd);
            }

            if (!visibleLayers.Any())
            {
                return;
            }

            using (var scope = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                foreach (var pair in visibleLayers)
                {
                    var path = string.Format("{0}/{1}", pair.Value, pair.Key.Name);
                    pair.Key.Visible = EditorGUILayout.ToggleLeft(path, pair.Key.Visible);
                }
                scrollPosition = scope.scrollPosition;
            }

            if (GUILayout.Button("Convert"))
            {
                if (!visibleLayers.Any())
                {
                    return;
                }

                var layers = visibleLayers.Keys.Where(x =>
                {
                    if ((int)x.Rect.width == 0 || (int)x.Rect.height == 0)
                    {
                        return false;
                    }
                    if (x.AdditionalInfo.Any(info => info is TextLayerInfo))
                    {
                        return false;
                    }
                    return true;
                }).ToArray();
                CreateGUI(EditorUtil.SaveAssets("Assets/GUI", layers));
            }
        }


        private Dictionary<Layer, string> GetHierarchyPath(PsdFile psd)
        {
            var dic = new Dictionary<Layer, string>();
            var stack = new Stack<string>();

            foreach (var layer in Enumerable.Reverse(psd.Layers))
            {
                var sectionInfo = (LayerSectionInfo)layer.AdditionalInfo.SingleOrDefault(x => x is LayerSectionInfo);
                if (sectionInfo == null)
                {
                    if ((int)layer.Rect.width == 0 || (int)layer.Rect.height == 0)
                    {
                        continue;
                    }
                    dic.Add(layer, string.Join("/", stack.Reverse().ToArray()));
                }
                else
                {
                    switch (sectionInfo.SectionType)
                    {
                        case LayerSectionType.Layer:
                        case LayerSectionType.OpenFolder:
                        case LayerSectionType.ClosedFolder:
                            stack.Push(layer.Name);
                            break;

                        case LayerSectionType.SectionDivider:
                            stack.Pop();
                            break;
                    }
                }
            }

            return dic.Reverse().ToDictionary(k => k.Key, v => v.Value);
        }

        private void CreateGUI(Sprite[] sprites)
        {
            GameObject canvasObj = GameObject.Find("Canvas");

            if (canvasObj == null)
            {
                canvasObj = new GameObject("Canvas");
                var canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            var root = new GameObject(originTexture.name, typeof(RectTransform));
            root.transform.SetParent(canvasObj.transform, false);
            root.GetComponent<RectTransform>().sizeDelta = originSize;

            var layerGroups = new Dictionary<string, List<Layer>>();
            foreach (var layer in visibleLayers)
            {
                var key = layer.Value;
                if (key.StartsWith(originTexture.name, StringComparison.CurrentCultureIgnoreCase))
                {
                    key = key.Remove(0, originTexture.name.Length);
                }
                if (!layerGroups.ContainsKey(key))
                {
                    layerGroups.Add(key, new List<Layer>());
                }
                layerGroups[key].Add(layer.Key);
            }

            var components = EditorUtil.ConvertLayerComponents(layerGroups, sprites);

            foreach (var component in components)
            {
                var posX = (component.Rect.min.x + component.Rect.width / 2f);
                var posY = originSize.y - (component.Rect.min.y + component.Rect.height / 2f);
                var rect = GetOrCreateTransform(root.transform, component.Path, component.Name, new Vector2(posX, posY));

                rect.sizeDelta = component.Rect.size;

                component.Create(rect);
            }
        }

        private RectTransform GetOrCreateTransform(Transform parent, string hierarchyPath, string name, Vector3 position)
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
