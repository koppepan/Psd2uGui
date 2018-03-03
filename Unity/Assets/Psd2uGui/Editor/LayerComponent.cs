using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PhotoshopFile;

namespace Psd2uGui.Editor
{
    internal abstract class LayerComponent
    {
        public string Name
        {
            get;
            private set;
        }
        public string Path
        {
            get;
            private set;
        }
        public Rect Rect
        {
            get;
            private set;
        }
        

        public LayerComponent(string name, string path, Rect rect)
        {
            Name = name;
            Path = path;
            Rect = rect;
        }

        public abstract void Create(RectTransform rect);
    }

    internal class TextLayerComponent : LayerComponent
    {
        private TextLayerInfo textInfo;

        public TextLayerComponent(string name, string path, Layer layer) : base(name, path, layer.Rect)
        {
            textInfo = (TextLayerInfo)layer.AdditionalInfo.FirstOrDefault(x => x is TextLayerInfo);
        }

        public override void Create(RectTransform rect)
        {
            var text = rect.gameObject.AddComponent<UnityEngine.UI.Text>();

            text.text = textInfo.Text;
            text.fontSize = (int)textInfo.FontSize;
            text.color = textInfo.FillColor;
            text.alignment = textInfo.Alignment;

            text.raycastTarget = false;

            // 2回セットしないと正しいサイズにならない
            rect.sizeDelta = new Vector2(text.preferredWidth, text.preferredHeight);
            rect.sizeDelta = new Vector2(text.preferredWidth, text.preferredHeight);
        }
    }

    internal class ImageLayerComponent : LayerComponent
    {
        Sprite sprite;

        public ImageLayerComponent(string name, string path, Rect rect, Sprite sprite) : base(name, path, rect)
        {
            this.sprite = sprite;
        }

        public override void Create(RectTransform rect)
        {
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();

            image.sprite = sprite;
            image.raycastTarget = false;
            image.SetNativeSize();

            rect.sizeDelta = Rect.size;
        }
    }

    internal class ButtonLayerComponent : LayerComponent
    {
        Sprite normal;
        Sprite highlighted;
        Sprite pressed;
        Sprite disabled;

        public ButtonLayerComponent(string name, string path, Rect rect, Sprite[] sprites) : base(name, path, rect)
        {
            normal = sprites.FirstOrDefault(x =>
            {
                var lower = x.name.ToLower();
                return !lower.EndsWith("highlighted") && !lower.EndsWith("pressed") && !lower.EndsWith("disabled");
            });
            highlighted = sprites.FirstOrDefault(x => x.name.ToLower().EndsWith("highlighted"));
            pressed = sprites.FirstOrDefault(x => x.name.ToLower().EndsWith("pressed"));
            disabled = sprites.FirstOrDefault(x => x.name.ToLower().EndsWith("disabled"));
        }

        public override void Create(RectTransform rect)
        {
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite = normal;
            image.raycastTarget = true;

            var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>();
            if (highlighted != null || pressed != null || disabled != null)
            {
                button.transition = UnityEngine.UI.Selectable.Transition.SpriteSwap;
                button.spriteState = new UnityEngine.UI.SpriteState
                {
                    highlightedSprite = highlighted,
                    pressedSprite = pressed,
                    disabledSprite = disabled
                };
            }

            rect.sizeDelta = Rect.size;
        }
    }
}
