/*
 * Based of the original Unity Toggle
*/

#if CLOCKWORK_AUDIO
using Quirks.Audio;
#endif

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using TMPro;

namespace Quirks.UI
{
    public class ClockworkToggle : Selectable, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Space]
        [Tooltip("The transition effect for toggling the toggle.")]
        public ToggleTransition toggleTransition = ToggleTransition.Fade;
        [Tooltip("The Graphic component that represents the toggle's visual state.")]
        public Graphic graphic;
        [Tooltip("Indicates whether the toggle is currently on.")]
        [SerializeField] bool IsOn;

        [Header("Hover Settings")]
        [Tooltip("Enable or disable hover effect.")]
        public bool onHoverEffect = true;

        [Space]
        [Tooltip("Determines if the hover graphic should be displayed when the toggle is interactable.")]
        public bool hoverGraphicIfInteractable = true;
        [Tooltip("A Graphic component that is displayed when the toggle is hovered over.")]
        public Graphic hoverGraphic;

#if CLOCKWORK_AUDIO
        [Space]
        [Tooltip("Determine if the hover sound effect should play when the toggle is interactable.")]
        public bool hoverSoundEffectIfInteractable = true;
        [Tooltip("An EffectPack component that plays a sound effect when the toggle is hovered over.")]
        public EffectPack hoverSoundEffect;
#endif

        [Header("Toggle Events")]
#if CLOCKWORK_AUDIO
        [Tooltip("An EffectPack component that plays a sound effect when the toggle is turned on.")]
        public EffectPack onValueChangedEffectOn;
        [Tooltip("An EffectPack component that plays a sound effect when the toggle is turned off.")]
        public EffectPack onValueChangedEffectOff;
        [Space]
#endif

        [Tooltip("A ToggleEvent that is invoked when the toggle's value changes.")]
        public ToggleEvent OnValueChanged = new ToggleEvent();

        /// <summary>Indicates whether the toggle is currently on.</summary>
        public bool isOn
        {
            get { return IsOn; }

            set
            {
                Set(value);
            }
        }

        // Initializes the toggle, setting the hover graphic's gameObject to inactive if its exists.
        protected override void Awake()
        {
            base.Awake();

            if (hoverGraphic)
                hoverGraphic.gameObject.SetActive(false);
        }

        protected override void Start()
        {
            PlayEffect(ToggleTransition.Instant);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            PlayEffect(ToggleTransition.Instant);
        }

        protected override void OnDisable()
        {
            base.OnEnable();

            if (hoverGraphic)
                hoverGraphic.gameObject.SetActive(false);
        }

        protected override void OnDidApplyAnimationProperties()
        {
            if (graphic != null)
            {
                bool oldValue = !Mathf.Approximately(graphic.canvasRenderer.GetColor().a, 0);
                if (IsOn != oldValue)
                {
                    IsOn = oldValue;
                    Set(!oldValue);
                }
            }

            base.OnDidApplyAnimationProperties();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            InternalToggle();
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

        /// <summary>Toggles the toggle's state and plays the appropriate sound effect if applicable.</summary>
        void InternalToggle()
        {
            if (!IsActive() || !IsInteractable())
                return;

            isOn = !isOn;

#if CLOCKWORK_AUDIO
            if (isOn)
                onValueChangedEffectOn?.Play();
            else
                onValueChangedEffectOff?.Play();
#endif
        }

        /// <summary>Plays the toggle effect based on the specified transition.</summary>
        void PlayEffect(ToggleTransition transition)
        {
            if (graphic == null)
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                graphic.canvasRenderer.SetAlpha(IsOn ? 1f : 0f);
            else
#endif

                switch (transition)
                {
                    case ToggleTransition.Instant:
                        graphic.CrossFadeAlpha(IsOn ? 1f : 0f, 0f, true);
                        break;
                    case ToggleTransition.FadeQuickly:
                        graphic.CrossFadeAlpha(IsOn ? 1f : 0f, 0.1f, true);
                        break;
                    case ToggleTransition.Fade:
                        graphic.CrossFadeAlpha(IsOn ? 1f : 0f, 0.25f, true);
                        break;
                }
        }

        /// <summary>Sets the toggle's state. Optionally invoking OnValueChanged event.</summary>
        void Set(bool value, bool sendCallback = true)
        {
            if (IsOn == value)
                return;

            IsOn = value;

            PlayEffect(toggleTransition);
            if (sendCallback)
            {
                OnValueChanged?.Invoke(IsOn);
            }
        }

        /// <summary>Sets the toggle's state without invoking the OnValueChanged event.</summary>
        public void SetIsOnWithoutNotify(bool value) => Set(value, false);

#if UNITY_EDITOR

        [UnityEditor.MenuItem("GameObject/UI/Clockwork/Toggle", false, 0)]
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
            buttonRect.sizeDelta = new Vector2(160, 20);

            ClockworkToggle toggle = go.AddComponent<ClockworkToggle>();

            RectTransform background = new GameObject("Background (Image)", typeof(RectTransform)).GetComponent<RectTransform>();
            background.anchorMin = new Vector2(0, 0.5f);
            background.anchorMax = new Vector2(0, 0.5f);
            background.sizeDelta = new Vector2(20, 20);
            background.localPosition = new Vector3(10, 0, 0);

            background.gameObject.AddComponent<Image>();
            background.transform.SetParent(toggle.transform, false);

            RectTransform toggleImage = new GameObject("Toggle (Image)", typeof(RectTransform)).GetComponent<RectTransform>();
            toggleImage.transform.SetParent(background.transform, false);

            toggleImage.anchorMin = new Vector2(0, 0);
            toggleImage.anchorMax = new Vector2(1f, 1f);
            toggleImage.sizeDelta = new Vector2(-4f, -4f);

            Image toggleIcon = toggleImage.gameObject.AddComponent<Image>();
            toggleIcon.color = Color.black;



            RectTransform textArea = new GameObject("Label (TMP)", typeof(RectTransform)).GetComponent<RectTransform>();
            textArea.anchorMin = new Vector2(0, 0f);
            textArea.anchorMax = new Vector2(1, 1f);
            textArea.sizeDelta = new Vector2(-48, 0);

            TextMeshProUGUI text = textArea.gameObject.AddComponent<TextMeshProUGUI>();
            text.transform.SetParent(toggle.transform, false);

            text.text = "Toggle";
            text.color = Color.black;

            text.fontSize = 14;
            text.alignment = TextAlignmentOptions.Left;


            toggle.graphic = toggleIcon;
            toggle.targetGraphic = background.GetComponent<Image>();

            UnityEditor.Selection.activeGameObject = go;
        }

#endif
    }

}
