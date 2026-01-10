using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssentialObject : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
