using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using TMPro;

namespace Quirks.UI
{
    public class AdvancedButton : SimpleButton
    {
        [Header("Tap and Hold")]
        [Tooltip("Enable Tap and Hold!\n(Only works with Left Click Events)")]
        public bool tapAndHold;

        [Space]
        [Tooltip("Time the button needs to be held for before initializing hold cycle.\n(Usually sooner than [Fast Tap Delay]")]
        public float initialTapDelay = 0;
        [Tooltip("Time waited between every hold cycle.\n(Usually greater than [Fast Hold Delay]")]
        public float holdDelay = 0.5f;

        [Space]
        [Tooltip("Time the button needs to be held for before initializing quick hold cycle.\n(Usually greater than [Initial Tap Delay]")]
        public float fastTapDelay = 0.4f;
        [Tooltip("Time waited between every quick hold cycle.\n(Usually lower than [Hold Delay]")]
        public float fastHoldDelay = 0.2f;

        // Tap and Hold States
        ButtonState tapHoldState = ButtonState.None;
        float tapHoldTimer = 0;
        float fastTapTimer = 0;

        private void Update()
        {
            HandleTapAndHold();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (tapHoldState == ButtonState.HoldDelay)
                return;

            base.OnPointerClick(eventData);
        }

        void HandleTapAndHold()
        {
            // Check that we want to call hold Event
            if (tapAndHold)
            {
                // Check that we are currently holding down the button
                if (IsPressed())
                {
                    // Use Time.unscaledDeltaTime to not be affected by TimeScale
                    float time = Time.unscaledDeltaTime;

                    // If Button State is None Trigger the Initial Tap Delay
                    if (tapHoldState == ButtonState.None)
                    {
                        tapHoldTimer = initialTapDelay;
                        tapHoldState = ButtonState.InitialTapDelay;
                    }
                    // If Button State is InitialTapDelay wait out the first delay to Trigger Hold & Trigger First Press
                    else if (tapHoldState == ButtonState.InitialTapDelay)
                    {
                        tapHoldTimer -= time;
                        if (tapHoldTimer <= 0)
                        {
                            tapHoldState = ButtonState.HoldDelay;
                            tapHoldTimer = holdDelay;
                            fastTapTimer = fastTapDelay;

                            PressLeftClick();
                        }
                    }
                    // If Button State is HoldDelay cycle through the tap delays to Trigger Press
                    else if (tapHoldState == ButtonState.HoldDelay)
                    {
                        tapHoldTimer -= time;
                        fastTapTimer -= time;

                        while (tapHoldTimer <= 0)
                        {
                            PressLeftClick();

                            if (fastTapTimer <= 0)
                                tapHoldTimer += fastHoldDelay;
                            else
                                tapHoldTimer += holdDelay;
                        }
                    }
                }
                else
                {
                    tapHoldState = ButtonState.None;
                }
            }

        }

#if UNITY_EDITOR

        [UnityEditor.MenuItem("GameObject/UI/Clockwork/Button (Advanced)", false, 0)]
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

            GameObject go = new GameObject("Clockwork (Advanced Button)");
            RectTransform buttonRect = go.AddComponent<RectTransform>();
            buttonRect.SetParent(parentCanvas.transform);

            buttonRect.localPosition = new Vector3(0, 0, 0);
            buttonRect.sizeDelta = new Vector2(160, 30);

            go.AddComponent<Image>();
            AdvancedButton button = go.AddComponent<AdvancedButton>();

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
