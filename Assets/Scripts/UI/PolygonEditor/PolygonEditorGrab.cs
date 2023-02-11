using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using EPPZ.Geometry.Model;

[RequireComponent(typeof(Image))]
public class PolygonEditorGrab : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler {
    public RectTransform RectTransform;
    public PolygonEditor Editor;
    public int VertexIndex;
    public Canvas Canvas;
    public AlpinePolygon polygon;
    
    //Offset stores the difference between the world pointer pos and world grab pos on click
    //This means an off center click doesn't snap the grab to the middle
    private Vector2 offset;

    //TODO: Figure out drag threshold and how to lower it
    public void OnBeginDrag(PointerEventData eventData)
    {
        offset = Camera.main.ScreenToWorldPoint(Input.mousePosition).ToHorizontal() - RectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //We dont use eventData.delta because it doesn't play nicely with world space canvases
        //TODO: reduce calls to camera main
        Vector2 newPos = Camera.main.ScreenToWorldPoint(Input.mousePosition).ToHorizontal() - offset;

        RectTransform.anchoredPosition = newPos;

        polygon.Polygon.points[VertexIndex] = newPos;
        polygon.Polygon.Calculate();

        Editor.Controller.MarkPolygonsDirty();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("OnPointerClick");
    }
}
