using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossRewardUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] GameObject rewardSlotPrefab;
    [SerializeField] Transform rewardContainer;
    [Header("Data")]
    [SerializeField] List<ItemBase> allPossibleRewards;

    [Header("Animation Settings")]
    [SerializeField] float appearSpeed = 0.3f; // Thời gian để 1 ô xuất hiện
    [SerializeField] float staggerDelay = 0.1f; // Thời gian trễ giữa các ô

    public event Action OnFinished;
    private bool isAnimating = false; // Biến để chặn người chơi bấm Z khi đang chạy animation

    public void ShowRewards()
    {
        this.gameObject.SetActive(true);
        StartCoroutine(ShowRewardsRoutine());
    }

    // Chuyển logic chính vào Coroutine để xử lý theo trình tự thời gian
    IEnumerator ShowRewardsRoutine()
    {
        isAnimating = true;

        // 1. Xóa phần thưởng cũ
        foreach (Transform child in rewardContainer) Destroy(child.gameObject);

        // 2. Chuẩn bị dữ liệu
        var inventory = Inventory.GetInventory();
        int typeCount = UnityEngine.Random.Range(1, 4);
        List<GameObject> createdSlots = new List<GameObject>();

        // 3. Tạo các slot nhưng chưa hiện lên ngay
        for (int i = 0; i < typeCount; i++)
        {
            var randomItem = allPossibleRewards[UnityEngine.Random.Range(0, allPossibleRewards.Count)];
            int amount = UnityEngine.Random.Range(1, 4);

            inventory.AddItem(randomItem, amount);

            var slotObj = Instantiate(rewardSlotPrefab, rewardContainer);
            slotObj.transform.Find("Icon").GetComponent<Image>().sprite = randomItem.Icon;
            slotObj.transform.Find("Count").GetComponent<Text>().text = "x" + amount;
            slotObj.transform.Find("Name").GetComponent<Text>().text = randomItem.Name;

            // --- ANIMATION SETUP: Đặt kích thước ban đầu về 0 để ẩn đi ---
            slotObj.transform.localScale = Vector3.zero;
            createdSlots.Add(slotObj);
            // -------------------------------------------------------------
        }

        // Đợi một chút cho UI ổn định
        yield return new WaitForSeconds(0.1f);

        // 4. Bắt đầu Animation xuất hiện lần lượt
        foreach (var slot in createdSlots)
        {
            // Gọi hiệu ứng nảy cho từng slot
            StartCoroutine(PopInElement(slot.transform));
            // Đợi một chút trước khi hiện slot tiếp theo (hiệu ứng staggered)
            yield return new WaitForSeconds(staggerDelay);
        }

        // Đợi cho đến khi slot cuối cùng hoàn thành animation
        yield return new WaitForSeconds(appearSpeed);

        isAnimating = false; // Cho phép bấm Z
    }

    // --- ANIMATION COROUTINE: Hiệu ứng nảy (Pop-in) ---
    IEnumerator PopInElement(Transform element)
    {
        float timer = 0f;
        // Giai đoạn 1: Phóng to từ 0 lên 1.1 (hơi quá khổ một chút để tạo độ nảy)
        while (timer < appearSpeed)
        {
            timer += Time.deltaTime;
            float t = timer / appearSpeed;
            // Sử dụng Lerp để thay đổi kích thước dần dần
            element.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.1f, t);
            yield return null; // Đợi đến frame tiếp theo
        }

        // Giai đoạn 2: Thu nhỏ về kích thước chuẩn 1.0
        timer = 0f;
        float bounceBackSpeed = 0.1f;
        while (timer < bounceBackSpeed)
        {
            timer += Time.deltaTime;
            float t = timer / bounceBackSpeed;
            element.localScale = Vector3.Lerp(Vector3.one * 1.1f, Vector3.one, t);
            yield return null;
        }

        // Đảm bảo kích thước cuối cùng là chính xác 1
        element.localScale = Vector3.one;
    }
    // --------------------------------------------------

    public void HandleUpdate()
    {
        // Chỉ cho phép bấm Z khi animation đã chạy xong
        if (!isAnimating && Input.GetKeyDown(KeyCode.Z))
        {
            this.gameObject.SetActive(false);
            OnFinished?.Invoke();
        }
    }
}