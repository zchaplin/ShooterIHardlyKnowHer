using UnityEngine;
using UnityEngine.SceneManagement;  // Necessary for scene management

public class Switcher : MonoBehaviour
{
    public void LoadScene()
    {
        Debug.Log("Balsd");
        SceneManager.LoadScene(1);  // Load the scene with the given name
    }
}
