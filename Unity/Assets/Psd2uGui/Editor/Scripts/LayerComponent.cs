using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        protected void SetParameter(string name, string path, Rect rect)
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

        public TextLayerComponent(string name, string path, Layer layer, Font font)
        {
            SetParameter(name, path, layer.Rect);

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

        public ImageLayerComponent(string name, string path, Rect rect, Sprite sprite)
        {
            SetParameter(name, path, rect);
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

        public ButtonLayerComponent(string name, string path, IEnumerable<Layer> layers, ButtonParameter param, Func<Layer, Sprite> getSprite)
        {
            var n = layers.FirstOrDefault(x => Regex.IsMatch(x.Name.ToLower(), param.NormalPattern));
            var p = layers.FirstOrDefault(x => Regex.IsMatch(x.Name.ToLower(), param.PressedPattern));
            var h = layers.FirstOrDefault(x => Regex.IsMatch(x.Name.ToLower(), param.HighlightedPattern));
            var d = layers.FirstOrDefault(x => Regex.IsMatch(x.Name.ToLower(), param.DisabledPattern));

            normal = getSprite(n);
            highlighted = getSprite(h);
            pressed = getSprite(p);
            disabled = getSprite(d);

            SetParameter(name, path, n.Rect);
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

    class ToggleLayerComponent : LayerComponent
    {
        Sprite background;
        Sprite checkmark;

        public ToggleLayerComponent(string name, string path, IEnumerable<Layer> layers, ToggleParameter param, Func<Layer, Sprite> getSprite)
        {
            var b = layers.FirstOrDefault(x => Regex.IsMatch(x.Name.ToLower(), param.BackgroundPattern));
            var c = layers.FirstOrDefault(x => Regex.IsMatch(x.Name.ToLower(), param.CheckmarkPattern));

            background = getSprite(b);
            checkmark = getSprite(c);

            SetParameter(name, path, b.Rect);
        }

        public override void Create(RectTransform rect)
        {
            var back = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            back.sprite = background;
            back.raycastTarget = true;

            var check = new GameObject(checkmark.name, typeof(RectTransform)).AddComponent<UnityEngine.UI.Image>();
            check.transform.SetParent(rect, false);
            check.sprite = checkmark;
            check.raycastTarget = false;
            check.SetNativeSize();

            var toggle = rect.gameObject.AddComponent<UnityEngine.UI.Toggle>();
            toggle.isOn = true;
            toggle.targetGraphic = back;
            toggle.graphic = check;

            rect.sizeDelta = Rect.size;
        }
    }

}
