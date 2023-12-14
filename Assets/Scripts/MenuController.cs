using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public void LoadScene(int sceneIndex)
    {
        GameController.Instance.LoadScene(sceneIndex);
    }

    public void ExitGame()
    {
        Application.Quit();

        Debug.Log("Quit Game");
    }
}
