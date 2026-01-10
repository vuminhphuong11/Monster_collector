using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDetail : MonoBehaviour
{
    [SerializeField] List<SceneDetail> connectedScene;
    public bool IsLoaded {  get; private set; }
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
            if(GameController.Instance.PreScene != null)
            {
                var previousLoadedScene = GameController.Instance.PreScene.connectedScene;
                foreach(var scene in previousLoadedScene)
                {
                    if (!connectedScene.Contains(scene)&&scene!=this)
                    {
                        scene.UnLoadScene();
                    }
                }
            }
        }
    }
    public void LoadScene()
    {
        if (!IsLoaded)
        {
            SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            IsLoaded = true;
        }
    }
    public void UnLoadScene()
    {
        if (IsLoaded)
        {
            SceneManager.UnloadSceneAsync(gameObject.name);
            IsLoaded =false;
        }
    }
}
