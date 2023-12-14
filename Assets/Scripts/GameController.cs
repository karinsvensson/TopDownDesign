using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : SingletonPersistent<GameController>
{
    private Animator anim;

    [SerializeField] float sceneTransitionTime = 1f;

    protected override void Awake()
    {
        base.Awake();

        anim = GetComponent<Animator>();

        anim.speed = 1f / sceneTransitionTime;

        anim.SetBool("Transitioning", false);
    }

    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(LoadSceneTransition(sceneIndex));
    }

    public void LoadNextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings) //If we're out of scenes, load the first one
        {
            nextSceneIndex = 0;
        }

        StartCoroutine(LoadSceneTransition(nextSceneIndex));
    }

    private IEnumerator LoadSceneTransition(int sceneIndex)
    {
        anim.SetBool("Transitioning", true);

        yield return new WaitForSeconds(sceneTransitionTime);

        SceneManager.LoadScene(sceneIndex);

        anim.SetBool("Transitioning", false);
    }
}
