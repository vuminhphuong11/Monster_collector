using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class LongGrass : MonoBehaviour, IPlayerTriggerable
{
    public void OnPlayerTriggered(PlayerController player)
    {
       
        if (UnityEngine.Random.Range(1, 101) <= 10) // 10% chance
            {
                Debug.Log("A wild Pokémon appeared!");

                GameController.Instance.StartBattle();
            }
        
    }
}
