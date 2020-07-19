using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    public void OnPlayButtonClick()
    {
        Debug.Log("Play Button Click");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
