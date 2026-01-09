using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] int lettersPerSecond ;
    [SerializeField] Color highlightedColor;
    [SerializeField] TMP_Text dialogText;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;
    [SerializeField] List<TMP_Text> actionTexts;
    [SerializeField] List<TMP_Text> moveTexts;
    [SerializeField] TMP_Text ppText;
    [SerializeField] TMP_Text typeText;
    [SerializeField] Color normalColor = Color.black; // Màu mặc định khi không chọn
    [SerializeField] Color disabledColor = Color.gray;
    // Biến lưu trạng thái nút Run có được dùng không
    private bool isRunEnabled = true;

    // Hàm này để BattleSystem gọi sang
    public void SetRunEnabled(bool enabled)
    {
        isRunEnabled = enabled;
    }
    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }
    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f/lettersPerSecond);
        }
        yield return new WaitForSeconds(0.5f);
    }
    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }
    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }
    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }
    public void UpdateActionSelection(int selectedAction)
    {
        for (int i = 0; i < actionTexts.Count; i++)
        {
            // Logic cho nút Run (nằm ở vị trí số 3)
            if (i == 3)
            {
                if (!isRunEnabled)
                {
                    // Nếu bị khóa -> Luôn luôn màu xám, kể cả khi đang chọn hay không
                    actionTexts[i].color = disabledColor;
                    continue; // Bỏ qua các lệnh bên dưới, sang vòng lặp tiếp theo
                }
            }

            // Logic tô màu bình thường
            if (i == selectedAction)
                actionTexts[i].color = highlightedColor;
            else
                actionTexts[i].color = normalColor;
        }
    }
    public void UpdateMoveSelection(int selectedMove, Move move)
    {
        for (int i = 0; i < moveTexts.Count; i++)
        {
            if (i == selectedMove)
            {
                moveTexts[i].color = highlightedColor;
            }
            else
            {
                moveTexts[i].color = Color.black;
            }
        }
        ppText.text = $"PP {move.PP}/{move.Base.PP}";
        typeText.text = move.Base.Type.ToString();
        if (move.PP == 0)
        {
            ppText.color = Color.red;
        }
        else
        {
            ppText.color = Color.black;
        }
    }
    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < moveTexts.Count; i++)
        {
            if (i < moves.Count)
            {
                moveTexts[i].text = moves[i].Base.Name;
            }
            else
            {
                moveTexts[i].text = "- - -";
            }
        }
    }
}
