using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    public void SceneChange(string name)
    {
        StartCoroutine(LoadSceneWithDelay(name));
    }

    private IEnumerator LoadSceneWithDelay(string sceneName)
    {
        Time.timeScale = 1;
        yield return new WaitForSeconds(0.4f);
        SceneManager.LoadScene(sceneName);
    }

    public void ExitApplication()
    {
        Application.Quit();
    }
}
