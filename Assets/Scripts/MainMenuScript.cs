using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class MainMenuScript : MonoBehaviour
{
    public Button QuitButton;
    public Button PlayButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        QuitButton.onClick.AddListener(quitApp);
        PlayButton.onClick.AddListener(loadGame);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void quitApp(){
        Application.Quit();
    }
    void loadGame(){
        SceneManager.LoadScene("GameScene");
    }
}
