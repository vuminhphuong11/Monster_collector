using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject menu;
    public event Action<int>onMenuSelected;
    public event Action OnBack;
    List<Text> menuItems;
    int selectedItem = 0;
    private void Awake()
    {
        menuItems = menu.GetComponentsInChildren<Text>().ToList();
    }
    public void OpenMenu()
    {
        menu.SetActive(true);
        UpdateItemSelection();
    }
    public void CloseMenu()
    {
        menu.SetActive(false);
    }
    public void HandleUpdate()
    {
        int prevSelection = selectedItem;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedItem += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedItem -= 2;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (selectedItem % 2 == 0) selectedItem++;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (selectedItem % 2 != 0) selectedItem--;
        }
        selectedItem = Mathf.Clamp(selectedItem, 0, menuItems.Count - 1);
        if (prevSelection != selectedItem)
        {
            UpdateItemSelection();
        }
        if(Input.GetKeyDown(KeyCode.Z))
        {
            onMenuSelected?.Invoke(selectedItem);
            CloseMenu();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            OnBack?.Invoke();
            CloseMenu();
        }
    }

    void UpdateItemSelection()
    {
        for (int i = 0; i < menuItems.Count; i++)
        {
            // Đổi màu: mục được chọn màu Đỏ, mục khác màu Đen
            menuItems[i].color = (i == selectedItem) ? GlobalSetting.i.HighlightedColor : Color.black;
        }
    }
    
}
