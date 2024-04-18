using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Quirks.UI
{
    public class DualSlider : ClockworkSlider
    {
        public override float normalizedValue 
        { 
            get => base.normalizedValue;
            set 
            {
                base.normalizedValue = value;
                delayTime = Time.realtimeSinceStartup + dualDelay;
            }
        }

        [Header("Dual Slider")]
        [Tooltip("A RectTransform for the dual slider's fill area.")]
        [SerializeField] RectTransform dualFillRect;

        [Space]
        [Tooltip("The transition mode for the dual slider's value.")]
        public TransitionType dualTransitionMode = TransitionType.Instant;

        [Tooltip("The speed of the transition effect.")]
        [SerializeField] float _dualTransitionSpeed = 1f;

        /// <summary>The speed of the transition effect.</summary>
        public float dualTransitionSpeed
        {
            get => _dualTransitionSpeed * Time.unscaledDeltaTime;
            set
            {
                _dualTransitionSpeed = value;
            }
        }

        [Space]
        [Tooltip("The delay time before updating the dual value.")]
        public float dualDelay = 0.4f;

        /// <summary>The current value of the dual slider.</summary>
        protected float dualValue;

        /// <summary>The normalized value of the dual slider (between 0 and 1)</summary>
        public float normalizedDualValue
        {
            get
            {
                if (Mathf.Approximately(minValue, maxValue))
                    return 0;

                return Mathf.InverseLerp(minValue, maxValue, dualValue);
            }
        }

        /// <summary>The time at which the dual value should be updated.</summary>
        float delayTime;

        // Cached References
        protected Image dualFillImage;

        protected override void UpdateCachedReferences()
        {
            base.UpdateCachedReferences();

            // Updates the cached references to dual fill rect and its image.
            if(dualFillRect && dualFillRect != (RectTransform)transform)
            {
                dualFillImage = dualFillRect.GetComponent<Image>();
            }
            else
            {
                dualFillRect = null;
                dualFillImage = null;
            }
        }

        // Updates the transition value and visuals of the slider.
        protected override void Update()
        {
            // Transition Value Different
            if (Time.realtimeSinceStartup >  delayTime)
                TransitionValue();

            TransitionDualValue();

            UpdateVisuals();
        }

        /// <summary>Updates the dual slider's value based on the transition mode.</summary>
        protected virtual void TransitionDualValue()
        {
            switch (dualTransitionMode)
            {
                case TransitionType.Instant:
                    dualValue = value;
                    break;
                case TransitionType.Lerp:
                    dualValue = Mathf.Lerp(dualValue, value, dualTransitionSpeed);
                    break;
                case TransitionType.EaseInOut:
                    float t = dualTransitionSpeed < 0.5f ? 2f * dualTransitionSpeed * dualTransitionSpeed : 1f - Mathf.Pow(-2f * dualTransitionSpeed + 2f, 2f) / 2f;
                    dualValue = Mathf.Lerp(dualValue, value, t);
                    break;
            }
        }

        protected override void UpdateVisuals()
        {
            base.UpdateVisuals();

            if(dualFillRect != null)
            {
                Vector2 anchorMin = Vector2.zero;
                Vector2 anchorMax = Vector2.one;

                if (dualFillImage != null && dualFillImage.type == Image.Type.Filled)
                {
                    dualFillImage.fillAmount = normalizedDualValue;
                }
                else
                {
                    // 0 = Horizontal
                    // 1 = Vertical
                    // TODO Make modular with Direction
                    if(normalizedDualValue > value)
                    {
                        anchorMin[(int)axis] = normalizedDualValue;
                        anchorMax[(int)axis] = fillRect.anchorMax.x;
                    }
                    else
                    {
                        anchorMax[(int)axis] = normalizedDualValue;
                        anchorMin[(int)axis] = fillRect.anchorMax.x;

                    }

                    /*if (reverseValue)
                        anchorMin[(int)axis] = 1 - normalizedDualValue;
                    else
                        anchorMax[(int)axis] = normalizedDualValue;*/
                }

                dualFillRect.anchorMin = anchorMin;
                dualFillRect.anchorMax = anchorMax;
            }
        }

#if UNITY_EDITOR

        [UnityEditor.MenuItem("GameObject/UI/Clockwork/Slider (Dual)", false, 0)]
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

            GameObject go = new GameObject("Clockwork (Dual Slider)");
            RectTransform sliderRect = go.AddComponent<RectTransform>();
            sliderRect.SetParent(parentCanvas.transform);

            sliderRect.localPosition = new Vector3(0, 0, 0);
            sliderRect.sizeDelta = new Vector2(160, 20);

            DualSlider slider = go.AddComponent<DualSlider>();

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

            RectTransform dualFill = new GameObject("Dual Fill", typeof(Image)).GetComponent<RectTransform>();
            dualFill.SetParent(fillArea.transform, false);
            dualFill.sizeDelta = new Vector2(0, 0);
            slider.dualFillRect = dualFill;

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
