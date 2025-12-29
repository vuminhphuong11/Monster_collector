using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    
    [SerializeField] TMP_Text messageText;
    PartyMemberUI[] memberSlots;

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>();
    }

    public void SetPartyData(List<Monster> monsters)
    {
        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < monsters.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(monsters[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }
        messageText.text = "Choose a Monster:";
    }
}
