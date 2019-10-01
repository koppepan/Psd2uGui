using UnityEngine;
using UnityEditor;

namespace Psd2uGui.Editor
{
    internal class PreviewWindow : EditorWindow
    {
        public static PreviewWindow Open()
        {
            var window = CreateInstance<PreviewWindow>();
            window.ShowModalUtility();

            return window;
        }

        Texture2D origin;

        public void Set(string title, Vector2 position, Texture2D origin)
        {
            this.origin = origin;

            this.title = title;
            this.position = new Rect(position + Vector2.up * 50, this.position.size);
            minSize = maxSize = new Vector2(origin.width, origin.height);
        }

        private void OnLostFocus()
        {
            Close();
        }

        private void OnGUI()
        {
            if (origin == null)
            {
                return;
            }
            EditorGUI.DrawTextureTransparent(new Rect(Vector2.zero, new Vector2(origin.width, origin.height)), origin);
        }
    }
}
