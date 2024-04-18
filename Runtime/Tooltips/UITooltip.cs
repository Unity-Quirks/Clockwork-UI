using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Quirks.UI
{
    public class UITooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Tooltip")]
        public GameObject tooltipPrefab;

        [Space]
        [TextArea(1, 30)] public string text = "";
        public float showDelay = 0.5f;

        GameObject current;

        /// <summary>Returns true if the tooltip is currently active.</summary>
        public bool IsActive() => current != null;
        /// <summary>Returns true if the tooltip is currently active and visible.</summary>
        public bool IsVisible() => IsActive() && current.gameObject.activeInHierarchy;

        public virtual void OnPointerEnter(PointerEventData eventData) => ShowToolTip();

        public virtual void OnPointerExit(PointerEventData eventData) => DestroyTooltip();
        protected virtual void OnDisable() => DestroyTooltip();
        protected virtual void OnDestroy() => DestroyTooltip(); 

        protected void ShowToolTip()
        {
            if(current == null)
            {
                StartCoroutine(CreateTooltip());
            }
        }

        protected virtual IEnumerator CreateTooltip()
        {
            yield return new WaitForSeconds(showDelay);

            if(tooltipPrefab == null)
            {
                Debug.LogWarning("Tooltip Prefab is not set!");
                yield return null;
            }
            else
            {
                current = Instantiate(tooltipPrefab, transform.position, Quaternion.identity);
                Transform uiParent = transform.root;

                current.transform.SetParent(uiParent, true);
                current.transform.SetAsLastSibling();

                current.GetComponentInChildren<TextMeshProUGUI>().text = text;

                StartCoroutine(PositionTooltip(current));
            }
        }

        // Reposition the tooltip after its content sizer triggered
        // Wait for next frame required to detect changes in rect transform
        protected virtual IEnumerator PositionTooltip(GameObject current)
        {
            yield return new WaitForEndOfFrame();

            RectTransform rect = current.GetComponent<RectTransform>();
            Vector2 myRectSize = transform.GetComponent<RectTransform>().rect.size;

            float offsetX = (myRectSize.x / 2f) + (rect.rect.width / 2f) - 4f;
            float offsetY = (myRectSize.y / 2f) + (rect.rect.height / 2f) - 4f;

            rect.position = new Vector3(current.transform.position.x + offsetX, current.transform.position.y - offsetY, 0);

            yield return null;
        }

        protected virtual void DestroyTooltip()
        {
            StopAllCoroutines();

            if (current != null)
                Destroy(current);
        }
    }

}

