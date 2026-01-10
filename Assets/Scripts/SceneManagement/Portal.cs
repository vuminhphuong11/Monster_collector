using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class Portal : MonoBehaviour,IPlayerTriggerable
{
    [SerializeField] int sceneToLoad=-1;
    [SerializeField] Transform spawnPoint;
    [SerializeField] DestinationIdentifier destinationPortal;
    PlayerController player;
    public void OnPlayerTriggered(PlayerController player)
    {
        this.player = player;
        Debug.Log("Player entered the portal");
        StartCoroutine(SwitchScene());
    }
    // Trong Portal.cs -> SwitchScene

    IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject);

        // 1. Khóa Input ngay khi chạm Portal
        GameController.Instance.PauseGame(true);

        yield return SceneManager.LoadSceneAsync(sceneToLoad);

        var destPortal = FindObjectsOfType<Portal>().First(x => x != this&& x.destinationPortal==this.destinationPortal);
        player.Character.StopMoving();
        player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);
        GameController.Instance.PauseGame(false);
        Destroy(gameObject);
    }
    public Transform SpawnPoint=>spawnPoint;

}
public enum DestinationIdentifier {A,B,C,D,E,F,G,H}
