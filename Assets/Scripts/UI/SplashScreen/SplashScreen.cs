using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour
{
    public float waitingTime;
	void Start ()
	{
        StartCoroutine(SplashScreenWaiter());
 	}
    
    IEnumerator SplashScreenWaiter()
    {
        yield return new WaitForSeconds(waitingTime);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // Loading the simulation scene
    }
}
