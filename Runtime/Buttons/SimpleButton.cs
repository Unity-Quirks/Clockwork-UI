#if CLOCKWORK_AUDIO
using Quirks.Audio;
#endif

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using TMPro;

namespace Quirks.UI
{
    public class SimpleButton : ClockworkButton
    {
        [Header("Right Click")]
#if CLOCKWORK_AUDIO
        [Tooltip("An EffectPack component that plays a sound effect when the button is right clicked.")]
        public EffectPack onRightClickSoundEffect;
#endif

        [Tooltip("A UnityEvent that is invoked when the button is right clicked.")]
        public UnityEvent OnRightClick;

        public override void OnPointerClick(PointerEventData eventData)
        {
            switch(eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    PressLeftClick();
                    return;
                case PointerEventData.InputButton.Right:
                    PressRightClick();
                    return;
            }
        }

        /// <summary>Invokes the OnRightClick event and plays the click sound effect if applicable.</summary>
        public void PressRightClick()
        {
            if (!IsActive() || !IsInteractable())
                return;

#if CLOCKWORK_AUDIO
            onRightClickSoundEffect?.Play();
#endif

            OnRightClick?.Invoke();
        }

#if UNITY_EDITOR

        [UnityEditor.MenuItem("GameObject/UI/Clockwork/Button (Simple)", false, 0)]
        static void CreateClockworkSlider(UnityEditor.MenuCommand menuCommand)
        {
            Canvas parentCanvas = null;

            // Check if we have a canvas selected and make this our parent canvas if we do
            if (menuCommand.context != null && menuCommand.context.GetType() == typeof(Canvas))
                parentCanvas = (Canvas)menuCommand.context;

            // If we don't have a parent canvas then find one
            if (parentCanvas == null)
                parentCanvas = FindObjectOfType<Canvas>();

            // If we still don't have a parent canvas then create one
            if (parentCanvas == null)
            {
                parentCanvas = new GameObject("Canvas", typeof(Canvas)).GetComponent<Canvas>();
                parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                parentCanvas.gameObject.AddComponent<CanvasScaler>();
                parentCanvas.gameObject.AddComponent<GraphicRaycaster>();

                EventSystem eventSystem = EventSystem.current;
                if (eventSystem == null)
                {
                    eventSystem = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
                    eventSystem.gameObject.AddComponent<StandaloneInputModule>();
                }
            }

            GameObject go = new GameObject("Clockwork (Simple Button)");
            RectTransform buttonRect = go.AddComponent<RectTransform>();
            buttonRect.SetParent(parentCanvas.transform);

            buttonRect.localPosition = new Vector3(0, 0, 0);
            buttonRect.sizeDelta = new Vector2(160, 30);

            go.AddComponent<Image>();
            SimpleButton button = go.AddComponent<SimpleButton>();

            RectTransform textArea = new GameObject("Text (TMP)", typeof(RectTransform)).GetComponent<RectTransform>();
            textArea.anchorMin = new Vector2(0, 0f);
            textArea.anchorMax = new Vector2(1, 1f);
            textArea.sizeDelta = new Vector2(0, 0);

            TextMeshProUGUI text = textArea.gameObject.AddComponent<TextMeshProUGUI>();
            text.transform.SetParent(button.transform, false);

            text.text = "Button";
            text.color = Color.black;

            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Center;

            button.targetGraphic = go.GetComponent<Image>();

            UnityEditor.Selection.activeGameObject = go;
        }

#endif
    }

}
