using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class GenericGrab : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler {
    public RectTransform RectTransform;
    public Canvas Canvas;
    
    //Offset stores the difference between the world pointer pos and world grab pos on click
    //This means an off center click doesn't snap the grab to the middle
    private Vector2 offset;

    public void OnBeginDrag(PointerEventData eventData) {
        offset = Camera.main.ScreenToWorldPoint(Input.mousePosition).ToHorizontal() - RectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData) {
        //We dont use eventData.delta because it doesn't play nicely with world space canvases
        //TODO: reduce calls to camera main
        Vector2 newPos = Camera.main.ScreenToWorldPoint(Input.mousePosition).ToHorizontal() - offset;

        RectTransform.anchoredPosition = newPos;

        OnDragBehavior(newPos);
    }
    
    public virtual void OnDragBehavior(Vector2 newPos) {}

    public void OnPointerClick(PointerEventData eventData) {
        
    }
}