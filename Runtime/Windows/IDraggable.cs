using UnityEngine.EventSystems;

namespace Quirks.UI
{
    public interface IDraggable : IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        void HandleDrag(PointerEventData d);

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            HandleDrag(eventData);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            HandleDrag(eventData);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            HandleDrag(eventData);
        }
    }
}