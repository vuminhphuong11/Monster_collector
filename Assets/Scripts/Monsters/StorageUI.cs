using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    int currentPartyIndex = 0;
    int currentBoxSlotIndex = 0;
    int currentBoxIndex = 0;

    // --- BIẾN SWAP ---
    bool isSwapping = false;
    int selectedMemberIndex = -1; // Lưu vị trí slot của con đang cầm
    int selectedBoxIndex = -1;    // Lưu trang Box của con đang cầm (nếu cầm từ Box)
    bool selectedFromParty = false; // True = Cầm từ Party, False = Cầm từ Box

    public void Init(MonsterParty party)
    {
        playerParty = party;
        storage = MonsterStorage.Instance;
        currentPartyIndex = 0;
        currentBoxSlotIndex = 0;
        state = StorageState.PartyFocus;
        isSwapping = false;
        Refresh();
    }

    void Refresh()
    {
        // 1. Cập nhật Party
        for (int i = 0; i < partySlots.Count; i++)
        {
            partySlots[i].gameObject.SetActive(true);
            if (i < playerParty.Monsters.Count)
                partySlots[i].SetData(playerParty.Monsters[i]);
            else
                partySlots[i].gameObject.SetActive(false); // Hoặc SetEmpty()
            partySlots[i].SetSelected(false);
            partySlots[i].SetNameColor(Color.black); // Reset màu chữ về đen
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
            pcSlots[i].SetNameColor(Color.black); // Reset màu chữ về đen
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
            if (currentPartyIndex < playerParty.Monsters.Count - 1)
                currentPartyIndex++;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentPartyIndex > 0)
                currentPartyIndex--;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            state = StorageState.BoxFocus;
            currentBoxSlotIndex = 0;
        }
        if (Input.GetKeyDown(KeyCode.Z)) OnPressSelect(true);
        if (Input.GetKeyDown(KeyCode.X)) OnBack();
        UpdateSelectionVisual();
    }

    void HandleBoxInput()
    {
        int itemsInCurrentBox = storage.GetMonstersInBox(currentBoxIndex).Count;
        int totalBoxes = storage.GetTotalBoxes();
        // --- DOWN ---
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentBoxSlotIndex < itemsInCurrentBox - 1)
            {
                currentBoxSlotIndex++;
            }
            else if (currentBoxIndex < totalBoxes - 1)
            {
                currentBoxIndex++;
                currentBoxSlotIndex = 0;
                Refresh();
            }
        }
        // --- UP ---
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentBoxSlotIndex > 0)
            {
                currentBoxSlotIndex--;
            }
            else if (currentBoxIndex > 0)
            {
                currentBoxIndex--;
                int itemsInPrevBox = storage.GetMonstersInBox(currentBoxIndex).Count;
                currentBoxSlotIndex = itemsInPrevBox > 0 ? itemsInPrevBox - 1 : 0;
                Refresh();
            }
        }
        // --- LEFT ---
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            state = StorageState.PartyFocus;
            currentPartyIndex = Mathf.Clamp(currentPartyIndex, 0, playerParty.Monsters.Count - 1);
        }
        // --- PAGE SWITCH ---
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (currentBoxIndex < totalBoxes - 1) { currentBoxIndex++; currentBoxSlotIndex = 0; Refresh(); }
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (currentBoxIndex > 0) { currentBoxIndex--; currentBoxSlotIndex = 0; Refresh(); }
        }
        if (Input.GetKeyDown(KeyCode.Z)) OnPressSelect(false);
        if (Input.GetKeyDown(KeyCode.X)) OnBack();
        UpdateSelectionVisual();
    }
    void UpdateRegenInfo()
    {
        Monster selectedMonster = null;

        // Xác định xem đang chọn con quái nào (Party hay Box)
        if (state == StorageState.PartyFocus)
        {
            if (currentPartyIndex < playerParty.Monsters.Count)
                selectedMonster = playerParty.Monsters[currentPartyIndex];
        }
        else if (state == StorageState.BoxFocus)
        {
            var boxMons = storage.GetMonstersInBox(currentBoxIndex);
            if (currentBoxSlotIndex < boxMons.Count)
                selectedMonster = boxMons[currentBoxSlotIndex];
        }

        // Cập nhật Text
        if (selectedMonster != null)
        {
            if (selectedMonster.HP < selectedMonster.MaxHP && selectedMonster.HP > 0)
            {
                // --- SỬA DÒNG NÀY: Gọi hàm tính tổng ---
                float totalTime = selectedMonster.GetTotalHealTimeLeft();

                // Hiển thị tổng thời gian
                // Ví dụ: "Full in: 45s"
                regenInfoText.text = $"Full in: {FormatTime(totalTime)}";
                regenInfoText.color = Color.yellow;
            }
            else if (selectedMonster.HP <= 0)
            {
                // Nếu đang hồi sinh (từ 0 máu)
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
        else
        {
            regenInfoText.text = "";
        }
    }
    string FormatTime(float timeInSeconds)
    {
        if (timeInSeconds < 60)
        {
            return $"{timeInSeconds.ToString("F1")}s"; // VD: 45.5s
        }
        else
        {
            // Nếu hơn 60s thì hiển thị dạng Phút:Giây (VD: 2m 15s)
            int minutes = Mathf.FloorToInt(timeInSeconds / 60);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60);
            return $"{minutes}m {seconds}s";
        }
    }
    void UpdateSelectionVisual()
    {
        // 1. Reset trạng thái Selected (Highlight khung)
        foreach (var s in partySlots) s.SetSelected(false);
        foreach (var s in pcSlots) s.SetSelected(false);
        // 2. Highlight con trỏ hiện tại
        if (state == StorageState.PartyFocus)
        {
            if (currentPartyIndex < partySlots.Count)
            {
                partySlots[currentPartyIndex].gameObject.SetActive(true);
                partySlots[currentPartyIndex].SetSelected(true);
                messageText.text = "Party Area";
            }
        }
        else
        {
            if (currentBoxSlotIndex < pcSlots.Count)
            {
                pcSlots[currentBoxSlotIndex].gameObject.SetActive(true);
                pcSlots[currentBoxSlotIndex].SetSelected(true);
                messageText.text = $"Page  {currentBoxIndex + 1}";
            }
        }
        // 3. XỬ LÝ MÀU CHỮ (GREEN) KHI ĐANG SWAP
        foreach (var s in partySlots) s.SetNameColor(Color.black);
        foreach (var s in pcSlots) s.SetNameColor(Color.black);

        if (isSwapping)
        {
            messageText.text = "Swap Mode: Choose destination";
            if (selectedFromParty)
            {
                // Nếu con đang cầm thuộc Party -> Đổi màu slot đó thành Xanh
                partySlots[selectedMemberIndex].SetNameColor(Color.green);
            }
            else
            {
                // Nếu con đang cầm thuộc Box -> Kiểm tra xem có đang ở đúng trang Box đó không
                if (currentBoxIndex == selectedBoxIndex)
                {
                    pcSlots[selectedMemberIndex].SetNameColor(Color.green);
                }
            }
        }
    }

    void OnPressSelect(bool isPartyArea)
    {
        if (!isSwapping)
        {
            // --- CHỌN LẦN 1 (PICK UP) ---
            if (isPartyArea)
            {
                selectedFromParty = true;
                selectedMemberIndex = currentPartyIndex;
            }
            else
            {
                // Không cho chọn ô trống trong Box để bắt đầu
                var boxMons = storage.GetMonstersInBox(currentBoxIndex);
                if (currentBoxSlotIndex >= boxMons.Count) return;

                selectedFromParty = false;
                selectedMemberIndex = currentBoxSlotIndex;
                selectedBoxIndex = currentBoxIndex; // Lưu lại trang Box hiện tại
            }
            isSwapping = true;
            UpdateSelectionVisual(); // Cập nhật để đổi màu chữ ngay
        }
        else
        {
            // --- CHỌN LẦN 2 (DROP / SWAP) ---

            // TH1: Cầm từ Party
            if (selectedFromParty)
            {
                if (isPartyArea) // Đích đến là Party -> Đổi chỗ nội bộ
                {
                    var temp = playerParty.Monsters[selectedMemberIndex];
                    playerParty.Monsters[selectedMemberIndex] = playerParty.Monsters[currentPartyIndex];
                    playerParty.Monsters[currentPartyIndex] = temp;
                }
                else // Đích đến là Box -> Cất vào Box (Deposit/Swap)
                {
                    storage.SwapMonster(currentBoxIndex, currentBoxSlotIndex, selectedMemberIndex, playerParty);
                }
            }
            // TH2: Cầm từ Box
            else
            {
                if (isPartyArea) // Đích đến là Party -> Rút ra (Withdraw/Swap)
                {
                    // Lưu ý: selectedBoxIndex là trang của con đang cầm
                    storage.SwapMonster(selectedBoxIndex, selectedMemberIndex, currentPartyIndex, playerParty);
                }
                else // Đích đến là Box
                {
                    if (currentBoxIndex != selectedBoxIndex)
                    {
                        messageText.text = "Cannot move between Boxes yet!";
                        return; // Chưa hỗ trợ chuyển giữa các trang Box
                    }
                    // Logic đổi chỗ nội bộ trong cùng 1 Box (nếu muốn làm thêm)
                    messageText.text = "Moved within Box!";
                }
            }

            // Kết thúc Swap, Reset lại
            isSwapping = false;
            selectedBoxIndex = -1;
            Refresh(); // Refresh để cập nhật lại dữ liệu và màu sắc
        }
    }

    void OnBack()
    {
        if (isSwapping)
        {
            // Nếu đang cầm quái mà bấm X -> Hủy thao tác
            isSwapping = false;
            Refresh(); // Reset màu chữ về đen
        }
        else
        {
            GameController.Instance.CloseStorage();
        }
    }
}