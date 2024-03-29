//>============================================================================<
//
//    Alpine, Ski Resort Tycoon Game
//    Copyright (C) 2024  Charles Bruel
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//>============================================================================<

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