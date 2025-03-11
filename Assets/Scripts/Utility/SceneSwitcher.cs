using UnityEngine;
using UnityEngine.SceneManagement;  // Necessary for scene management

public class SceneSwitcher : MonoBehaviour
{
    public void LoadScene()
    {
        SceneManager.LoadScene(1);  
    }

    public void LoadEndScene()
    {
        SceneManager.LoadScene(2); 
    }
}
