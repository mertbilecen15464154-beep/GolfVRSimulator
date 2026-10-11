using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneRestart : MonoBehaviour
{
    [Header("Input Ayarý")]
    // Sað el A tuþu için: <XRController>{RightHand}/primaryButton
    public InputActionReference restartAction;

    private void OnEnable()
    {
        if (restartAction != null)
            restartAction.action.Enable();
    }

    private void OnDisable()
    {
        if (restartAction != null)
            restartAction.action.Disable();
    }

    void Update()
    {
        // 1. XR Controller Kontrolü (Mevcut yapýn)
        bool xrPressed = restartAction != null && restartAction.action.WasPressedThisFrame();

        // 2. Klavye "R" Tuþu Kontrolü
        bool rKeyPressed = Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;

        // Ýkisinden birine basýldýysa sahneyi yeniden baþlat
        if (xrPressed || rKeyPressed)
        {
            RestartCurrentScene();
        }
    }

    public void RestartCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}