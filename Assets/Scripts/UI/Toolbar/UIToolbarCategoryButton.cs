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

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIToolbarCategoryButton : MonoBehaviour {
    public Sprite Sprite;
    public GameObject UIToolbarCategoryPrefab;
    public GameObject UIToolbarItemPrefab;
    public List<IUIToolbarItemProvider> UIToolbarItemProviders = new List<IUIToolbarItemProvider>();
    public UIReferences UIReferences;

    [NonSerialized]
    public UIToolbarCategory UIToolbarCategory;

    public void Inflate() {
        RectTransform ownRect = GetComponent<RectTransform>();

        Transform Image = transform.Find("Image");
        Transform Panel = transform.Find("Panel");

        Image.GetComponent<Image>().sprite = Sprite;

        GameObject temp = Instantiate(UIToolbarCategoryPrefab);
        UIToolbarCategory = temp.AddComponent<UIToolbarCategory>();
        UIToolbarCategory.UIToolbarItemPrefab = UIToolbarItemPrefab;
        UIToolbarCategory.UIToolbarItemProviders = UIToolbarItemProviders;

        UIToolbarCategory.transform.SetParent(transform);
        RectTransform rect = UIToolbarCategory.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(150, -ownRect.anchoredPosition.y - 300);
        rect.localScale = new Vector3(1, 1, 1);
        
        UIToolbarCategory.UIReferences = UIReferences;

        UIToolbarCategory.Inflate();
        UIToolbarCategory.gameObject.SetActive(false);
    }
}