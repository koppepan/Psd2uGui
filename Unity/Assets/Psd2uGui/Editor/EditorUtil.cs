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
    internal static class EditorUtil
    {

        public static List<LayerComponent> ConvertLayerComponents(Dictionary<string, List<Layer>> groups, Sprite[] sprites)
        {
            IEnumerable<LayerComponent> components = new List<LayerComponent>();
            foreach (var pair in groups)
            {
                components = components.Concat(ConvertLayers(pair.Key, new List<Layer>(pair.Value), sprites));
            }
            return components.ToList();
        }

        static IEnumerable<LayerComponent> ConvertLayers(string path, List<Layer> layers, Sprite[] sprites)
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
                        var buttonSprites = sprites.Where(x => buttons.Any(y => string.Equals(x.name, y.Name, StringComparison.CurrentCultureIgnoreCase))).ToArray();
                        components.Add(new ButtonLayerComponent(groupName, path.Remove(path.Length - (groupName.Length + 1)), buttons.First().Rect, buttonSprites));

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
                    var sprite = sprites.FirstOrDefault(x => string.Equals(x.name, layer.Name, StringComparison.CurrentCultureIgnoreCase));
                    components.Add(new ImageLayerComponent(layer.Name, path, layer.Rect, sprite));
                }
            }

            return components;
        }

        public static Sprite[] SaveAssets(string saveFolderPath, Layer[] layers)
        {
            List<Sprite> atlas = new List<Sprite>();

            foreach (var layer in layers)
            {
                var sprite = atlas.FirstOrDefault(x => string.Equals(x.name, layer.Name));
                if (sprite != null)
                {
                    if (sprite.rect.width < layer.Rect.width || sprite.rect.height < layer.Rect.height)
                    {
                        atlas.Remove(sprite);
                    }
                }
                sprite = SaveAsset(string.Format("{0}/{1}.png", saveFolderPath, layer.Name), CreateTexture(layer));
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

            AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.mipmapEnabled = false;
            textureImporter.isReadable = false;

            AssetDatabase.ImportAsset(path);
            return (Sprite)AssetDatabase.LoadAssetAtPath(path, typeof(Sprite));
        }

        static Texture2D CreateTexture(Layer layer)
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

        static Color32[] LayerToColors(Layer layer)
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

    }
}
