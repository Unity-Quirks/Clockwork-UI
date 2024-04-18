/*
 * Based of the original Unity Button
*/

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
    public class ClockworkButton : Selectable, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Hover Settings")]
        [Tooltip("Enable or disable hover effects.")]
        public bool onHoverEffect;

        [Space]
        [Tooltip("Determine if the hover graphic should be displayed when the button is interactable.")]
        public bool hoverGraphicIfInteractable = true;
        [Tooltip("A Graphic component that is displayed when the button is hovered over.")]
        public Graphic hoverGraphic;

#if CLOCKWORK_AUDIO
        [Space]
        [Tooltip("Determine if the hover sound effect should play when the button is interactable.")]
        public bool hoverSoundEffectIfInteractable = true;
        [Tooltip("An EffectPack component that plays a sound effect when the button is hovered over.")]
        public EffectPack hoverSoundEffect;
#endif

        [Header("Left Click")]
#if CLOCKWORK_AUDIO
        [Tooltip("An EffectPack component that plays a sound effect when the button is clicked.")]
        public EffectPack onClickSoundEffect;
#endif

        [Tooltip("A UnityEvent that is invoked when the button is clicked.")]
        public UnityEvent OnClick;

        // Initialize the button, setting the hover graphics gameObject to inactive if it exists.
        protected override void Awake()
        {
            base.Awake();

            if (hoverGraphic)
                hoverGraphic.gameObject.SetActive(false);
        }

        // Ensures the hover graphics gameObject is set to inactive when the button is disabled.
        protected override void OnDisable()
        {
            base.OnEnable();

            if (hoverGraphic)
                hoverGraphic.gameObject.SetActive(false);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                PressLeftClick();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            if (onHoverEffect)
            {
                if (hoverGraphic)
                {
                    if (IsInteractable() || !hoverGraphicIfInteractable)
                    {
                        hoverGraphic.gameObject.SetActive(true);
                    }
                }


#if CLOCKWORK_AUDIO
                if (IsInteractable() || !hoverSoundEffectIfInteractable)
                    hoverSoundEffect?.Play();
#endif
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            if (onHoverEffect)
            {
                if (hoverGraphic)
                    hoverGraphic.gameObject.SetActive(false);
            }
        }

        /// <summary>Invokes the OnClick event and plays the click sound effect if applicable.</summary>
        public void PressLeftClick()
        {
            if (!IsActive() || !IsInteractable())
                return;

#if CLOCKWORK_AUDIO
            onClickSoundEffect?.Play();
#endif

            OnClick?.Invoke();
        }

#if UNITY_EDITOR

        [UnityEditor.MenuItem("GameObject/UI/Clockwork/Button", false, 0)]
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

            GameObject go = new GameObject("Clockwork (Button)");
            RectTransform buttonRect = go.AddComponent<RectTransform>();
            buttonRect.SetParent(parentCanvas.transform);

            buttonRect.localPosition = new Vector3(0, 0, 0);
            buttonRect.sizeDelta = new Vector2(160, 30);

            go.AddComponent<Image>();
            ClockworkButton button = go.AddComponent<ClockworkButton>();

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
