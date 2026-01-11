using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDetail : MonoBehaviour
{
    [SerializeField] List<SceneDetail> connectedScene;
    public bool IsLoaded {  get; private set; }
    List<SavableEntity> savableEntities;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Debug.Log($"Entered {gameObject.name}");
            LoadScene();
            GameController.Instance.SetCurrentScene(this);
            foreach (var scene in connectedScene)
            {
                scene.LoadScene();
            }
            // unload scene
            var prevScene = GameController.Instance.PreScene;
            if (GameController.Instance.PreScene != null)
            {
                var previousLoadedScene = GameController.Instance.PreScene.connectedScene;
                foreach(var scene in previousLoadedScene)
                {
                    if (!connectedScene.Contains(scene)&&scene!=this)
                    {
                        scene.UnLoadScene();
                    }
                    if (!connectedScene.Contains(prevScene))//&& prevScene != this)
                    {
                        prevScene.UnLoadScene();
                    }
                }
            }
        }
    }
    public void LoadScene()
    {
        if (!IsLoaded)
        {
            var operation =SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            IsLoaded = true;

            operation.completed += (AsyncOperation op) =>
            {
                savableEntities = GetSavableEntitiesInScene();
                SavingSystem.i.RestoreEntityStates(savableEntities);
            };

        }
    }
    public void UnLoadScene()
    {
        if (IsLoaded)
        {
            SavingSystem.i.CaptureEntityStates(savableEntities);

            SceneManager.UnloadSceneAsync(gameObject.name);
            IsLoaded =false;
        }
    }
    List<SavableEntity> GetSavableEntitiesInScene()
    {
        var currScene = SceneManager.GetSceneByName(gameObject.name);
        var SavableEntities = FindObjectsOfType<SavableEntity>().Where(x => x.gameObject.scene == currScene).ToList();
        return SavableEntities;
    }
}
