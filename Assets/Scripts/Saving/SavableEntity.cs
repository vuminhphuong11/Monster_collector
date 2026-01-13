using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class SavableEntity : MonoBehaviour
{
    [SerializeField] string uniqueId = "";
    static Dictionary<string, SavableEntity> globalLookup = new Dictionary<string, SavableEntity>();

    public string UniqueId => uniqueId;

    // Giữ nguyên logic CaptureState
    public object CaptureState()
    {
        Dictionary<string, object> state = new Dictionary<string, object>();
        foreach (ISavable savable in GetComponents<ISavable>())
        {
            state[savable.GetType().ToString()] = savable.CaptureState();
        }
        return state;
    }

    // Giữ nguyên logic RestoreState
    public void RestoreState(object state)
    {
        Dictionary<string, object> stateDict = (Dictionary<string, object>)state;
        foreach (ISavable savable in GetComponents<ISavable>())
        {
            string id = savable.GetType().ToString();

            if (stateDict.ContainsKey(id))
                savable.RestoreState(stateDict[id]);
        }
    }

#if UNITY_EDITOR
    // ĐỔI TỪ UPDATE SANG ONVALIDATE ĐỂ TỐI ƯU VÀ BẮT LỖI
    private void OnValidate()
    {
        // Không chạy khi đang Play game
        if (Application.IsPlaying(gameObject)) return;

        // Không chạy cho Prefab chưa kéo vào scene (tránh lỗi null path)
        if (String.IsNullOrEmpty(gameObject.scene.path)) return;

        // --- BẮT ĐẦU BẪY LỖI ---
        try
        {
            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty property = serializedObject.FindProperty("uniqueId");

            if (String.IsNullOrEmpty(property.stringValue) || !IsUnique(property.stringValue))
            {
                property.stringValue = Guid.NewGuid().ToString();
                serializedObject.ApplyModifiedProperties();
            }

            globalLookup[property.stringValue] = this;
        }
        catch (System.Exception ex)
        {
            // NẾU CÓ LỖI, NÓ SẼ CHẠY VÀO ĐÂY VÀ BÁO TÊN VẬT THỂ
            string objectName = gameObject.name;
            string parentName = transform.parent != null ? transform.parent.name : "None";

            Debug.LogError($"[BẮT ĐƯỢC THỦ PHẠM] Lỗi tại GameObject: '{objectName}' (Cha: {parentName}). " +
                           $"Scene: {gameObject.scene.name}. " +
                           $"Chi tiết lỗi: {ex.Message}");
        }
    }
#endif

    private bool IsUnique(string candidate)
    {
        if (!globalLookup.ContainsKey(candidate)) return true;

        if (globalLookup[candidate] == this) return true;

        if (globalLookup[candidate] == null)
        {
            globalLookup.Remove(candidate);
            return true;
        }

        if (globalLookup[candidate].UniqueId != candidate)
        {
            globalLookup.Remove(candidate);
            return true;
        }

        return false;
    }
}