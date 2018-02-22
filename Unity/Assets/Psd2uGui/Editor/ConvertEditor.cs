using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using PhotoshopFile;

namespace Psd2uGui.Editor
{
    public class ConvertEditor : EditorWindow
    {
        static Dictionary<string, Layer[]> groups;
        Vector2 scrollPosition;

        [MenuItem("Convert/Convert")]
        static void Open()
        {
            var psd = new PsdFile(Application.dataPath + "/../../SamplePsd/Sample.psd", new LoadContext { Encoding = System.Text.Encoding.Default });
            groups = EditorUtil.GetLayerGroup(psd);

            var win = GetWindow<ConvertEditor>();
            win.Show();
        }

        private void OnGUI()
        {
            using (var scope = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                foreach (var group in groups)
                {
                    foreach (var layer in group.Value)
                    {
                        layer.Visible = EditorGUILayout.ToggleLeft(string.Format(" {0}/{1}", group.Key, layer.Name), layer.Visible);
                    }
                }
                scrollPosition = scope.scrollPosition;
            }

            if (GUILayout.Button("Convert"))
            {
                var textures = new Dictionary<string, Texture2D>();
                foreach (var layer in groups.SelectMany(x => x.Value))
                {
                    if (!layer.Visible || layer.Rect == Rect.zero)
                    {
                        continue;
                    }
                    if (layer.AdditionalInfo.Any(x => x is TextLayerInfo))
                    {
                        continue;
                    }

                    var path = string.Format("Assets/GUI/{0}.png", layer.Name);
                    if (!textures.ContainsKey(path))
                    {
                        textures.Add(path, EditorUtil.CreateTexture(layer));
                    }
                }
                EditorUtil.SaveAssets(textures);
            }
        }
    }
}
