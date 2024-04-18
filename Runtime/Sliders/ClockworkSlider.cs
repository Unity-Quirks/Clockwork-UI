/*
 * Based of the original Unity Slider
*/

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Quirks.UI
{
    [ExecuteAlways]
    public class ClockworkSlider : Selectable, IDragHandler, IInitializePotentialDragHandler
    {
        [Header("UI Elements")]
        [Tooltip("A RectTransform for the slider's fill area.")]
        [SerializeField] protected RectTransform fillRect;
        [Tooltip("A RectTransform for the slider's handle.")]
        [SerializeField] protected RectTransform handleRect;

        [Space]
        [Tooltip("Determines the slider's direction.")]
        [SerializeField] protected Direction _direction = Direction.LeftToRight;

        /// <summary>Determines the slider's direction.</summary>
        public Direction direction
        {
            get => _direction;
            set
            {
                if (_direction != value)
                {
                    _direction = value;
                    UpdateVisuals();
                }
            }
        }

        public enum Direction
        {
            LeftToRight,
            RightToLeft
        }

        [Header("Base Slider Values")]
        [Tooltip("The minimum value of the slider.")]
        [SerializeField] protected float _minValue = 0f;
        /// <summary>The minimum value of the slider.</summary>
        public float minValue
        {
            get => _minValue;
            set
            {
                float min = value;
                min = Mathf.Min(min, _maxValue);

                _minValue = min;
            }
        }

        [Tooltip("The maximum value of the slider.")]
        [SerializeField] protected float _maxValue = 1f;
        /// <summary>The maximum value of the slider.</summary>
        public float maxValue
        {
            get => _maxValue;
            set
            {
                float max = value;
                max = Mathf.Max(max, _minValue);

                _maxValue = max;
            }
        }

        [Space]
        [Tooltip("Determines if values should use only whole numbers.")]
        [SerializeField] protected bool wholeNumbers;
        [Tooltip("The current value of the slider.")]
        [SerializeField] protected float _value;
        /// <summary>The current value of the slider.</summary>
        public float value
        {
            get => _value;
            set
            {
                float oldV = _value;

                float v = value;
                v = Mathf.Clamp(v, _minValue, _maxValue);

                _value = v;

                if(oldV != v)
                    OnValueChanged?.Invoke(v);
            }
        }

        /// <summary>The normalized value of the slider (between 0 and 1)</summary>
        public virtual float normalizedValue
        {
            get
            {
                if (Mathf.Approximately(minValue, maxValue))
                    return 0;

                return Mathf.InverseLerp(minValue, maxValue, transitionValue);
            }
            set
            {
                this.value = Mathf.Lerp(minValue, maxValue, value);
            }
        }

        [Header("Transition")]
        [Tooltip("The transition mode for the slider's value.")]
        public TransitionType transitionMode = TransitionType.Instant;

        [Tooltip("The speed of the transition effect.")]
        [SerializeField] float _transitionSpeed = 1f;

        /// <summary>The speed of the transition effect.</summary>
        public float transitionSpeed
        {
            get => _transitionSpeed * Time.unscaledDeltaTime;
            set
            {
                _transitionSpeed = value;
            }
        }

        /// <summary>The current value of the transition.</summary>
        protected float transitionValue = 0f;
        public enum TransitionType { Instant, Lerp, EaseInOut }

        [Space]
        [Tooltip("A UnityEvent that is invoked when the slider's value changes.")]
        public UnityEvent<float> OnValueChanged = new UnityEvent<float>();

        // Cached References
        protected Image fillImage;
        protected Transform fillTransform;
        protected RectTransform fillContainerRect;
        protected Transform handleTransform;
        protected RectTransform handleContainerRect;

        protected enum Axis { Horizontal = 0, Vertical = 1 }
        protected Axis axis => _direction == Direction.LeftToRight || _direction == Direction.RightToLeft ? Axis.Horizontal : Axis.Vertical;
        protected bool reverseValue => _direction == Direction.RightToLeft;


        // Initializes the slider, setting the transition value and updating the cached references.
        protected override void OnEnable()
        {
            base.OnEnable();

            transitionValue = value;
            UpdateCachedReferences();
        }

        // Updates the transition value and visuals of the slider.
        protected virtual void Update()
        {
            TransitionValue();
            UpdateVisuals();
        }

        /// <summary>Updates the slider's value based on the transition mode.</summary>
        protected virtual void TransitionValue()
        {
            switch (transitionMode)
            {
                case TransitionType.Instant:
                    transitionValue = value;
                    break;
                case TransitionType.Lerp:
                    transitionValue = Mathf.Lerp(transitionValue, value, transitionSpeed);
                    break;
                case TransitionType.EaseInOut:
                    float t = transitionSpeed < 0.5f ? 2f * transitionSpeed * transitionSpeed : 1f - Mathf.Pow(-2f * transitionSpeed + 2f, 2f) / 2f;
                    transitionValue = Mathf.Lerp(transitionValue, value, t);
                    break;
            }
        }

        /// <summary>Determines if the slider can be dragged based on the event data.</summary>
        bool MayDrag(PointerEventData eventData)
        {
            return IsActive() && IsInteractable() && eventData.button == PointerEventData.InputButton.Left;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            base.OnPointerDown(eventData);

            UpdateDrag(eventData, eventData.pressEventCamera);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            UpdateDrag(eventData, eventData.pressEventCamera);
        }

        /// <summary>Updates cached references to the fill and handle rects.</summary>
        protected virtual void UpdateCachedReferences()
        {
            if(fillRect && fillRect != (RectTransform)transform)
            {
                fillTransform = fillRect.transform;
                fillImage = fillRect.GetComponent<Image>();

                if(fillTransform.parent != null)
                    fillContainerRect = fillTransform.parent.GetComponent<RectTransform>();
            }
            else
            {
                fillRect = null;
                fillContainerRect = null;
                fillImage = null;
            }

            if(handleRect && handleRect != (RectTransform)transform)
            {
                handleTransform = handleRect.transform;

                if(handleTransform.parent != null)
                    handleContainerRect = handleTransform.parent.GetComponent<RectTransform>();
            }
            else
            {
                handleRect = null;
                handleContainerRect = null;
            }
        }

        /// <summary>Sets the slider's value. Optionally invoking OnValueChanged event.</summary>
        protected virtual void Set(float input, bool sendCallback = true)
        {
            float newValue = ClampValue(input);

            // If our value has not changed
            // We can return otherwise we update ^^
            if (_value == newValue)
                return;

            _value = newValue;

            if(sendCallback)
            {
                OnValueChanged.Invoke(newValue);
            }
        }

        /// <summary>Update the visuals of the slider based on its current value.</summary>
        protected virtual void UpdateVisuals()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UpdateCachedReferences();
#endif

            if(fillContainerRect != null)
            {
                Vector2 anchorMin = Vector2.zero;
                Vector2 anchorMax = Vector2.one;

                if(fillImage != null && fillImage.type == Image.Type.Filled)
                {
                    fillImage.fillAmount = normalizedValue;
                }
                else
                {
                    // 0 = Horizontal
                    // 1 = Vertical
                    // TODO Make modular with Direction
                    if (reverseValue)
                        anchorMin[(int)axis] = 1 - normalizedValue;
                    else
                        anchorMax[(int)axis] = normalizedValue;
                }

                fillRect.anchorMin = anchorMin;
                fillRect.anchorMax = anchorMax;
            }

            if (handleContainerRect != null)
            {
                Vector2 anchorMin = Vector2.zero;
                Vector2 anchorMax = Vector2.one;

                anchorMin[(int)axis] = anchorMax[(int)axis] = (reverseValue ? (1 - normalizedValue) : normalizedValue);

                handleRect.anchorMin = anchorMin;
                handleRect.anchorMax = anchorMax;
            }

        }

        /// <summary>Updates the drag based on the event data and camera.</summary>
        protected void UpdateDrag(PointerEventData eventData, Camera cam)
        {
            RectTransform clickRect = handleContainerRect ?? fillContainerRect;
            if (clickRect != null && clickRect.rect.size[(int)axis] > 0)
            {
                Vector2 position = eventData.position;

                Vector2 localCursor;
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(clickRect, position, cam, out localCursor))
                    return;

                localCursor -= clickRect.rect.position;

                float val = Mathf.Clamp01(localCursor[(int)axis] / clickRect.rect.size[(int)axis]);
                normalizedValue = (reverseValue ? 1f - val : val);
            }
        }

        /// <summary>Clamps the input value to the slider's range.</summary>
        protected float ClampValue(float input)
        {
            float newValue = Mathf.Clamp(input, minValue, maxValue);
            if(wholeNumbers)
                newValue = Mathf.Round(newValue);

            return newValue;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (wholeNumbers)
            {
                _minValue = Mathf.Round(_minValue);
                _maxValue = Mathf.Round(_maxValue);
            }

            _value = ClampValue(_value);

            Set(_value, false);
            transitionValue = value; 
        }
#endif

        /// <summary>Initializes potential drag events, disabling the drag thershold.</summary>
        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

#if UNITY_EDITOR

        [UnityEditor.MenuItem("GameObject/UI/Clockwork/Slider", false, 0)]
        static void CreateClockworkSlider(UnityEditor.MenuCommand menuCommand)
        {
            Canvas parentCanvas = null;

            // Check if we have a canvas selected and make this our parent canvas if we do
            if(menuCommand.context != null && menuCommand.context.GetType() == typeof(Canvas))
                parentCanvas = (Canvas)menuCommand.context;

            // If we don't have a parent canvas then find one
            if(parentCanvas == null)
                parentCanvas = FindObjectOfType<Canvas>();

            // If we still don't have a parent canvas then create one
            if(parentCanvas == null)
            {
                parentCanvas = new GameObject("Canvas", typeof(Canvas)).GetComponent<Canvas>();
                parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                parentCanvas.gameObject.AddComponent<CanvasScaler>();
                parentCanvas.gameObject.AddComponent<GraphicRaycaster>();

                EventSystem eventSystem = EventSystem.current;
                if(eventSystem == null)
                {
                    eventSystem = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
                    eventSystem.gameObject.AddComponent<StandaloneInputModule>();
                }
            }

            GameObject go = new GameObject("Clockwork (Slider)");
            RectTransform sliderRect = go.AddComponent<RectTransform>();
            sliderRect.SetParent(parentCanvas.transform);

            sliderRect.localPosition = new Vector3(0, 0, 0);
            sliderRect.sizeDelta = new Vector2(160, 20);

            ClockworkSlider slider = go.AddComponent<ClockworkSlider>();

            RectTransform background = new GameObject("Background", typeof(Image)).GetComponent<RectTransform>();

            background.GetComponent<Image>().color = new Color(0.75f, 0.75f, 0.75f);
            background.SetParent(go.transform, false);
            background.anchorMin = new Vector2(0, 0.25f);
            background.anchorMax = new Vector2(1, 0.75f);
            background.sizeDelta = new Vector2(0, 0);

            RectTransform fillArea = new GameObject("Fill Area", typeof(RectTransform)).GetComponent<RectTransform>();
            fillArea.anchorMin = new Vector2(0, 0.25f);
            fillArea.anchorMax = new Vector2(1, 0.75f);
            fillArea.SetParent(go.transform, false);
            fillArea.sizeDelta = new Vector2(0, 0);

            RectTransform fill = new GameObject("Slider Fill", typeof(Image)).GetComponent<RectTransform>();
            fill.SetParent(fillArea.transform, false);
            fill.sizeDelta = new Vector2(0, 0);
            slider.fillRect = fill;

            RectTransform handleArea = new GameObject("Handle Area", typeof(RectTransform)).GetComponent<RectTransform>();
            handleArea.anchorMin = new Vector2(0, 0f);
            handleArea.anchorMax = new Vector2(1, 1f);
            handleArea.SetParent(go.transform, false);
            handleArea.sizeDelta = new Vector2(-20, 0);

            RectTransform handle = new GameObject("Handle", typeof(Image)).GetComponent<RectTransform>();
            handle.SetParent(handleArea.transform, false);
            handle.sizeDelta = new Vector2(20, 0);
            slider.handleRect = handle;

            slider.targetGraphic = handle.GetComponent<Image>();

            UnityEditor.Selection.activeGameObject = go;
        }

#endif
    }
}