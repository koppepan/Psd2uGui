using UnityEngine;

namespace Psd2uGui.Editor
{
    class ConvertParameter : ScriptableObject
    {
        public string textureSavePath = "Assets/GUI";
        public Font defaultFont = null;

        public string labelKey = "label_.*";

        public ButtonParameter Button = null;
        public ToggleParameter Toggle = null;
    }

    [System.Serializable]
    class ButtonParameter
    {
        public string Pattern = "";

        public string NormalPattern = "";
        public string PressedPattern = "";
        public string HighlightedPattern = "";
        public string DisabledPattern = "";
    }

    [System.Serializable]
    class ToggleParameter
    {
        public string Pattern = "";
        public string BackgroundPattern = "";
        public string CheckmarkPattern = "";
    }
}
