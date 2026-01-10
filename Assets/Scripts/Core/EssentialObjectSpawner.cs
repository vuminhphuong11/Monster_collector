using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssentialObjectSpawner : MonoBehaviour
{
    [SerializeField] GameObject essentialObjectsPrefab;
    private void Awake()
    {
        // Tìm xem đã có prefab này trong scene chưa
        // Giả sử prefab của bạn có chứa script EssentialObject
        var existingObjects = FindObjectsOfType<EssentialObject>();

        if (existingObjects.Length == 0)
        {
            // Nếu chưa có thì mới tạo mới
            Instantiate(essentialObjectsPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        }
    }
}
