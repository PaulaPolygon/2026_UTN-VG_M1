using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicio : MonoBehaviour
{

    public GameObject panelCreditos; 

    public void Jugar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    
    public void MostrarCreditos()
    {
        panelCreditos.SetActive(true); 
    }

    public void OcultarCreditos()
    {
        panelCreditos.SetActive(false); 
    }
}

