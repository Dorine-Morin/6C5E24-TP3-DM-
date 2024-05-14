using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public void LoadScene(string name) {
//        Debug.Log("Scene : " + name);
        SceneManager.LoadScene(name);
    }
}
