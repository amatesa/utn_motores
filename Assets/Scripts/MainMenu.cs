using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
 
    public GameObject mainMenu;
    public GameObject optionsMenu;


    //boton para opciones
    public void OpenOptionsPanel()
    {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }


    //boton de back
    public void CloseOptionsPanel()
    {
        mainMenu.SetActive(true);
        optionsMenu.SetActive(false);
    }



    public void QuitGame()
    {
        Application.Quit();
    }

    //boton de play para ir al nivel 1
    public void PlayGame()
        {
        SceneManager.LoadScene("Level_01");
        }
}
