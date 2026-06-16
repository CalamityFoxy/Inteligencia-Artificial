using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public void TransitionToGame()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
