using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using PhotoshopFile;

namespace Psd2uGui.Editor
{
    static class EditorUtil
    {

        public static Dictionary<string, Layer[]> GetLayerGroup(PsdFile psd)
        {
            var stack = new Stack<string>();
            var dic = new Dictionary<string, List<Layer>>();

            foreach (var layer in Enumerable.Reverse(psd.Layers))
            {
                var sectionInfo = (LayerSectionInfo)layer.AdditionalInfo.SingleOrDefault(x => x is LayerSectionInfo);
                if (sectionInfo == null)
                {
                    if (!layer.Visible || layer.Rect == Rect.zero)
                    {
                        continue;
                    }

                    var key = string.Join("/", stack.Reverse().ToArray());
                    if (dic.ContainsKey(key))
                    {
                        dic[key].Add(layer);
                    }
                    else
                    {
                        dic.Add(key, new List<Layer> { layer });
                    }
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

            return dic.Reverse().ToDictionary(k => k.Key, v => v.Value.ToArray());

        }

        public static List<LayerComponent> ConvertLayerComponents(Dictionary<string, Layer[]> groups)
        {
            IEnumerable<LayerComponent> components = new List<LayerComponent>();
            foreach (var pair in groups)
            {
                components = components.Concat(ConvertLayers(pair.Key, new List<Layer>(pair.Value)));
            }
            return components.ToList();
        }

        static IEnumerable<LayerComponent> ConvertLayers(string path, List<Layer> layers)
        {
            List<LayerComponent> components = new List<LayerComponent>();

            var groupName = path.Split('/').Length > 1 ? path.Split('/').Last() : "";

            if (!string.IsNullOrEmpty(groupName))
            {
                if (Regex.IsMatch(groupName.ToLower(), "button_.*"))
                {
                    var buttons = layers.Where(x => Regex.IsMatch(x.Name.ToLower(), ".*button_.*")).ToArray();
                    if (buttons.Any())
                    {
                        components.Add(new ButtonLayerComponent(groupName, path, buttons));

                        foreach (var b in buttons)
                        {
                            layers.Remove(b);
                        }
                    }
                }
            }

            foreach (var layer in layers)
            {
                if (Regex.IsMatch(layer.Name.ToLower(), "label_.*"))
                {
                    components.Add(new TextLayerComponent(layer.Name, path, layer));
                }
                else
                {
                    components.Add(new ImageLayerComponent(layer.Name, path, layer));
                }
            }

            return components;
        }

        public static Color32[] LayerToColors(Layer layer)
        {
            Color32[] pixels = new Color32[(int)layer.Rect.width * (int)layer.Rect.height];

            Channel red = layer.Channels.GetId(0);
            Channel green = layer.Channels.GetId(1);
            Channel blue = layer.Channels.GetId(2);
            Channel alpha = layer.AlphaChannel;

            for (int i = 0; i < pixels.Length; i++)
            {
                byte r = red.ImageData[i];
                byte g = green.ImageData[i];
                byte b = blue.ImageData[i];
                byte a = 255;
                if (alpha != null)
                {
                    a = alpha.ImageData[i];
                }
                int mod = i % (int)layer.Rect.width;
                int n = (((int)layer.Rect.width - mod - 1) + i) - mod;
                pixels[pixels.Length - n - 1] = new Color32(r, g, b, a);
            }
            return pixels;
        }

        public static Texture2D CreateTexture(Layer layer)
        {
            if (!layer.Visible)
            {
                return null;
            }
            if ((int)layer.Rect.width == 0 || (int)layer.Rect.height == 0)
            {
                return null;
            }

            Texture2D tex = new Texture2D((int)layer.Rect.width, (int)layer.Rect.height, TextureFormat.RGBA32, true);

            tex.SetPixels32(LayerToColors(layer));
            tex.Apply();
            return tex;
        }

        public static Sprite[] SaveAssets(Dictionary<string, Texture2D> textures)
        {
            List<Sprite> atlas = new List<Sprite>();
            foreach (var pair in textures)
            {
                var sprite = SaveAsset(pair.Key, pair.Value);
                atlas.Add(sprite);
            }
            return atlas.ToArray();
        }

         static Sprite SaveAsset(string path, Texture2D tex)
        {
            FileInfo file = new FileInfo(path);
            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }

            byte[] buf = tex.EncodeToPNG();
            File.WriteAllBytes(path, buf);
            AssetDatabase.ImportAsset(path);

            // Load the texture so we can change the type
            AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.mipmapEnabled = false;
            textureImporter.isReadable = false;

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            return (Sprite)AssetDatabase.LoadAssetAtPath(path, typeof(Sprite));
        }
    }
}
