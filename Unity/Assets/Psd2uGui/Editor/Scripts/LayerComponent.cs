using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PhotoshopFile;

namespace Psd2uGui.Editor
{
    abstract class LayerComponent
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

    class TextLayerComponent : LayerComponent
    {
        private TextLayerInfo textInfo;
        private Font font;

        public TextLayerComponent(string name, string path, Layer layer, Font font) : base(name, path, layer.Rect)
        {
            this.font = font;
            textInfo = (TextLayerInfo)layer.AdditionalInfo.FirstOrDefault(x => x is TextLayerInfo);
        }

        public override void Create(RectTransform rect)
        {
            var text = rect.gameObject.AddComponent<UnityEngine.UI.Text>();

            text.font = font;
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

    class ImageLayerComponent : LayerComponent
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

    class ButtonLayerComponent : LayerComponent
    {
        Sprite normal;
        Sprite highlighted;
        Sprite pressed;
        Sprite disabled;

        public ButtonLayerComponent(string name, string path, Rect rect, Sprite normal, Sprite pressed, Sprite highlighted, Sprite disabled) : base(name, path, rect)
        {
            this.normal = normal;
            this.pressed = pressed;
            this.highlighted = highlighted;
            this.disabled = disabled;
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
