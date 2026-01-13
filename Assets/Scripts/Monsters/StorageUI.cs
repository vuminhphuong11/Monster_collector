using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public enum StorageState { PartyFocus, BoxFocus, Busy }

public class StorageUI : MonoBehaviour
{
    [Header("Party Area (Left - Max 3)")]
    [SerializeField] List<PartyMemberUI> partySlots;

    [Header("PC Box Area (Right - Max 6)")]
    [SerializeField] List<PartyMemberUI> pcSlots;
    [SerializeField] TMP_Text messageText;
    [SerializeField] TMP_Text regenInfoText;

    MonsterParty playerParty;
    MonsterStorage storage;
    StorageState state;
    string customMessage = "";

    int currentPartyIndex = 0;
    int currentBoxSlotIndex = 0;
    int currentBoxIndex = 0;

    // --- BIẾN SWAP ---
    bool isSwapping = false;
    int selectedMemberIndex = -1;
    int selectedBoxIndex = -1;
    bool selectedFromParty = false;

    //  BIẾN CHO CHẾ ĐỘ CHỌN (ITEM) ---
    bool selectionMode = false;
    Action<Monster> onSelectedHandler;
    Action onBackHandler;

    public void Init(MonsterParty party)
    {
        playerParty = party;
        storage = MonsterStorage.Instance;
        currentPartyIndex = 0;
        currentBoxSlotIndex = 0;
        state = StorageState.PartyFocus;
        isSwapping = false;

        selectionMode = false;
        onSelectedHandler = null;
        onBackHandler = null;
 

        Refresh();
    }

    public void EnableSelectionMode(MonsterParty party, Action<Monster> onSelected, Action onBack)
    {
        playerParty = party;
        storage = MonsterStorage.Instance;

        selectionMode = true;
        onSelectedHandler = onSelected;
        onBackHandler = onBack;

        state = StorageState.PartyFocus;
        currentPartyIndex = 0;

        // Đảm bảo không bị dính trạng thái Swap cũ
        isSwapping = false;

        Refresh();
    }

    void Refresh()
    {
        // ... (GIỮ NGUYÊN CODE CẬP NHẬT UI CỦA BẠN) ...
        for (int i = 0; i < partySlots.Count; i++)
        {
            partySlots[i].gameObject.SetActive(true);
            if (i < playerParty.Monsters.Count)
                partySlots[i].SetData(playerParty.Monsters[i]);
            else
                partySlots[i].gameObject.SetActive(false);
            partySlots[i].SetSelected(false);
            partySlots[i].SetNameColor(Color.black);
        }

        List<Monster> boxMonsters = storage.GetMonstersInBox(currentBoxIndex);
        for (int i = 0; i < pcSlots.Count; i++)
        {
            pcSlots[i].gameObject.SetActive(true);
            if (i < boxMonsters.Count)
                pcSlots[i].SetData(boxMonsters[i]);
            else
                pcSlots[i].gameObject.SetActive(false);

            pcSlots[i].SetSelected(false);
            pcSlots[i].SetNameColor(Color.black);
        }
        UpdateSelectionVisual();
    }

    public void HandleUpdate()
    {
        if (state == StorageState.PartyFocus) HandlePartyInput();
        else if (state == StorageState.BoxFocus) HandleBoxInput();
        UpdateRegenInfo();
    }

    void HandlePartyInput()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            customMessage = "";
            if (currentPartyIndex < playerParty.Monsters.Count - 1) currentPartyIndex++;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            customMessage = "";
            if (currentPartyIndex > 0) currentPartyIndex--;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            customMessage = "";
            state = StorageState.BoxFocus;
            currentBoxSlotIndex = 0;
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (selectionMode)
            {
                // CHẾ ĐỘ DÙNG ITEM
                if (currentPartyIndex < playerParty.Monsters.Count)
                {
                    // 1. Gọi sang Inventory (Hàm này sẽ set text "Used Potion...")
                    onSelectedHandler?.Invoke(playerParty.Monsters[currentPartyIndex]);

                    // 2. MẸO QUAN TRỌNG: Lưu lại dòng chữ vừa được set
                    string msgFromInventory = messageText.text;

                    // 3. Refresh giao diện (Lúc này text sẽ bị xóa về mặc định)
                    Refresh();

                    // 4. Khôi phục lại dòng chữ đã lưu
                    messageText.text = msgFromInventory;

                    // 5. Chuyển chế độ
                    selectionMode = false;
                }
            }
            else
            {
                // CHẾ ĐỘ SWAP (Hoạt động như bình thường)
                OnPressSelect(true);
            }
        }

        // --- LOGIC PHÍM B: Quay lại (Back/Cancel) ---
        if (Input.GetKeyDown(KeyCode.B))
        {
            // Nếu đang cầm Pokemon (Swap) thì thả xuống
            if (isSwapping)
            {
                isSwapping = false;
                Refresh();
            }
            // Nếu có đường về Inventory thì về Inventory
            else if (onBackHandler != null)
            {
                onBackHandler.Invoke();
            }
            // Nếu mở bình thường thì đóng
            else
            {
                GameController.Instance.CloseStorage();
            }
        }
        // --- LOGIC PHÍM X: Thoát hẳn (Exit/Close All) ---
        if (Input.GetKeyDown(KeyCode.X))
        {
            GameController.Instance.CloseStorage();
        }

        UpdateSelectionVisual();
    }

    void HandleBoxInput()
    {
        int itemsInCurrentBox = storage.GetMonstersInBox(currentBoxIndex).Count;
        int totalBoxes = storage.GetTotalBoxes();
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentBoxSlotIndex < itemsInCurrentBox - 1) currentBoxSlotIndex++;
            else if (currentBoxIndex < totalBoxes - 1) { currentBoxIndex++; currentBoxSlotIndex = 0; Refresh(); }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentBoxSlotIndex > 0) currentBoxSlotIndex--;
            else if (currentBoxIndex > 0) { currentBoxIndex--; int itemsInPrevBox = storage.GetMonstersInBox(currentBoxIndex).Count; currentBoxSlotIndex = itemsInPrevBox > 0 ? itemsInPrevBox - 1 : 0; Refresh(); }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            state = StorageState.PartyFocus;
            currentPartyIndex = Mathf.Clamp(currentPartyIndex, 0, playerParty.Monsters.Count - 1);
        }
        if (Input.GetKeyDown(KeyCode.W)) { if (currentBoxIndex < totalBoxes - 1) { currentBoxIndex++; currentBoxSlotIndex = 0; Refresh(); } }
        if (Input.GetKeyDown(KeyCode.Q)) { if (currentBoxIndex > 0) { currentBoxIndex--; currentBoxSlotIndex = 0; Refresh(); } }

        // --- SỬA ĐỔI LOGIC BẤM Z ---
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (selectionMode)
            {
                var boxMons = storage.GetMonstersInBox(currentBoxIndex);
                if (currentBoxSlotIndex < boxMons.Count)
                {
                    // 1. Dùng item
                    onSelectedHandler?.Invoke(boxMons[currentBoxSlotIndex]);

                    // 2. Lưu text
                    string msgFromInventory = messageText.text;

                    // 3. Refresh
                    Refresh();

                    // 4. Khôi phục text
                    messageText.text = msgFromInventory;

                    // 5. Chuyển chế độ
                    selectionMode = false;
                }
            }
            else
            {
                OnPressSelect(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            // Nếu đang cầm Pokemon (Swap) thì thả xuống
            if (isSwapping)
            {
                isSwapping = false;
                Refresh();
            }
            // Nếu có đường về Inventory thì về Inventory
            else if (onBackHandler != null)
            {
                onBackHandler.Invoke();
            }
            // Nếu mở bình thường thì đóng
            else
            {
                GameController.Instance.CloseStorage();
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            GameController.Instance.CloseStorage();
        }

        UpdateSelectionVisual();
    }
    void UpdateRegenInfo()
    { 
        Monster selectedMonster = null;
        if (state == StorageState.PartyFocus && currentPartyIndex < playerParty.Monsters.Count) selectedMonster = playerParty.Monsters[currentPartyIndex];
        else if (state == StorageState.BoxFocus)
        {
            var boxMons = storage.GetMonstersInBox(currentBoxIndex);
            if (currentBoxSlotIndex < boxMons.Count) selectedMonster = boxMons[currentBoxSlotIndex];
        }

        if (selectedMonster != null)
        {
            // Giữ nguyên logic hiển thị text hồi máu của bạn
            if (selectedMonster.HP < selectedMonster.MaxHP && selectedMonster.HP > 0)
            {
                float totalTime = selectedMonster.GetTotalHealTimeLeft();
                regenInfoText.text = $"Full in: {FormatTime(totalTime)}";
                regenInfoText.color = Color.yellow;
            }
            else if (selectedMonster.HP <= 0)
            {
                float totalTime = selectedMonster.GetTotalHealTimeLeft();
                regenInfoText.text = $"Revive in: {FormatTime(totalTime)}";
                regenInfoText.color = Color.red;
            }
            else
            {
                regenInfoText.text = "Fully Healed";
                regenInfoText.color = Color.green;
            }
        }
        else regenInfoText.text = "";
    }
    string FormatTime(float timeInSeconds)
    { 
        if (timeInSeconds < 60) return $"{timeInSeconds.ToString("F1")}s";
        else { int m = Mathf.FloorToInt(timeInSeconds / 60); int s = Mathf.FloorToInt(timeInSeconds % 60); return $"{m}m {s}s"; }
    }

    void UpdateSelectionVisual()
    {
        // 1. Reset màu sắc (Giữ nguyên code cũ)
        foreach (var s in partySlots) { s.SetSelected(false); s.SetNameColor(Color.black); }
        foreach (var s in pcSlots) { s.SetSelected(false); s.SetNameColor(Color.black); }

        // 2. Highlight ô đang chọn (Giữ nguyên code cũ)
        if (state == StorageState.PartyFocus)
        {
            if (currentPartyIndex < partySlots.Count)
            {
                partySlots[currentPartyIndex].gameObject.SetActive(true);
                partySlots[currentPartyIndex].SetSelected(true);
            }
        }
        else
        {
            if (currentBoxSlotIndex < pcSlots.Count)
            {
                pcSlots[currentBoxSlotIndex].gameObject.SetActive(true);
                pcSlots[currentBoxSlotIndex].SetSelected(true);
            }
        }

        // --- PHẦN LOGIC HIỂN THỊ TEXT (SỬA LẠI ĐOẠN NÀY) ---

        // Ưu tiên 1: Nếu có tin nhắn tùy chỉnh (ví dụ: Used Potion...) -> Hiển thị nó
        if (!string.IsNullOrEmpty(customMessage))
        {
            messageText.text = customMessage;
        }
        // Ưu tiên 2: Nếu đang Swap
        else if (isSwapping && !selectionMode)
        {
            messageText.text = "Swap Mode: Choose destination";
            if (selectedFromParty) partySlots[selectedMemberIndex].SetNameColor(Color.green);
            else if (currentBoxIndex == selectedBoxIndex) pcSlots[selectedMemberIndex].SetNameColor(Color.green);
        }
        // Ưu tiên 3: Mặc định
        else
        {
            if (state == StorageState.PartyFocus)
                messageText.text = selectionMode ? "Select Pokemon" : "Your Team!";
            else
                messageText.text = selectionMode ? "Select Pokemon" : $"Page {currentBoxIndex + 1}";
        }
    }

    void OnPressSelect(bool isPartyArea)
    {
        if (!isSwapping)
        {
            if (isPartyArea) { selectedFromParty = true; selectedMemberIndex = currentPartyIndex; }
            else
            {
                var boxMons = storage.GetMonstersInBox(currentBoxIndex);
                if (currentBoxSlotIndex >= boxMons.Count) return;
                selectedFromParty = false; selectedMemberIndex = currentBoxSlotIndex; selectedBoxIndex = currentBoxIndex;
            }
            isSwapping = true;
            UpdateSelectionVisual();
        }
        else
        {
            if (selectedFromParty)
            {
                if (isPartyArea)
                {
                    var temp = playerParty.Monsters[selectedMemberIndex];
                    playerParty.Monsters[selectedMemberIndex] = playerParty.Monsters[currentPartyIndex];
                    playerParty.Monsters[currentPartyIndex] = temp;
                }
                else storage.SwapMonster(currentBoxIndex, currentBoxSlotIndex, selectedMemberIndex, playerParty);
            }
            else
            {
                if (isPartyArea) storage.SwapMonster(selectedBoxIndex, selectedMemberIndex, currentPartyIndex, playerParty);
                else
                {
                    if (currentBoxIndex != selectedBoxIndex) { messageText.text = "Cannot move between Box yet!"; return; }
                    messageText.text = "Moved within Box!";
                }
            }
            isSwapping = false; selectedBoxIndex = -1; Refresh();
        }
    }

    void OnBack()
    {
        // 1. Nếu đang Swap dở -> Hủy Swap
        if (isSwapping)
        {
            isSwapping = false;
            Refresh();
        }
        // 2. Nếu có Handler (nghĩa là mở từ Inventory) -> Quay về Inventory
        // Lưu ý: Dù selectionMode đã bị tắt (false) nhưng onBackHandler vẫn còn giá trị
        else if (onBackHandler != null)
        {
            onBackHandler.Invoke();
        }
        // 3. Nếu mở bình thường -> Tắt UI
        else
        {
            GameController.Instance.CloseStorage();
        }
    }
    public void SetMessageText(string text)
    {
        // Lưu lại tin nhắn để không bị UpdateSelectionVisual xóa mất
        customMessage = text;
        messageText.text = text;
    }
}