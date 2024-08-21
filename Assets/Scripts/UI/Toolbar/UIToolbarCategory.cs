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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIToolbarCategory : MonoBehaviour {
    public GameObject UIToolbarItemPrefab;
    public List<IUIToolbarItemProvider> UIToolbarItemProviders;
    public UIReferences UIReferences;

    public void Inflate() {
        Transform ScrollAreaTools = transform.Find("Scroll Area").Find("Tools");
        for(int i = 0; i < UIToolbarItemProviders.Count; i++) {
            IUIToolbarItemProvider provider = UIToolbarItemProviders[i];

            GameObject temp = Instantiate(UIToolbarItemPrefab);
            temp.transform.SetParent(ScrollAreaTools);
            RectTransform rect = temp.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(i * 75, 0);
            rect.localScale = new Vector3(1, 1, 1);

            temp.transform.Find("Image").GetComponent<Image>().sprite = provider.GetSprite();
            Button button = temp.transform.Find("Image").GetComponent<Button>();
            button.onClick.AddListener(() => provider.OnToolEnabled(UIReferences));
        }

        ScrollAreaTools.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 75 * UIToolbarItemProviders.Count);
    }
}