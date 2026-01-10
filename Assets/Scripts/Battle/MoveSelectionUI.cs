using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MoveSelectionUI : MonoBehaviour
{
    [SerializeField] List<Text> moveTexts;// 10 move ke ca move moi hoc
    [SerializeField] Color highLightColor;
    int currentSelection = 0;
    int totalItems = 0;
    public void SetMoveData(List<MoveBase>currentMoves,MoveBase NewMove)
    {
        for(int i = 0;i<currentMoves.Count;i++)
        {
            moveTexts[i].text = currentMoves[i].Name;
        }
        moveTexts[currentMoves.Count].text = NewMove.Name;
        totalItems = currentMoves.Count + 1;

    }
    public void HandleMoveSelection(Action<int> onSelected)
    {
        // 1. Đi sang PHẢI (Chỉ đi được nếu đang ở cột trái và ô bên phải có dữ liệu)
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentSelection % 2 == 0) // Nếu đang ở cột chẵn (cột trái)
            {
                if (currentSelection + 1 < totalItems)
                    currentSelection++;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentSelection % 2 != 0) // Nếu đang ở cột lẻ (cột phải)
            {
                currentSelection--;
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentSelection + 2 < totalItems)
            {
                currentSelection += 2;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentSelection - 2 >= 0)
            {
                currentSelection -= 2;
            }
        }
        UpdateMoveSelection(currentSelection);
        if (Input.GetKeyDown(KeyCode.Z))
        {
            onSelected?.Invoke(currentSelection);
        }
    }

    public void UpdateMoveSelection(int selection)
    {
        for (int i = 0; i < moveTexts.Count; i++) // Duyệt qua toàn bộ Text có trong List
        {
            if (i == selection)
            {
                moveTexts[i].color = highLightColor;
            }
            else
            {
                moveTexts[i].color = Color.black;
            }
        }
    }
}
