using UnityEngine;

public class GhostTrigger : MonoBehaviour
{
    public bool topIcerdeMi = false;

    // Eðer top hayalet sopanýn collider'ýna girerse
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("GolfBall"))
        {
            topIcerdeMi = true;
        }
    }

    // Eðer top hayalet sopanýn collider'ýndan çýkarsa
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("GolfBall"))
        {
            topIcerdeMi = false;
        }
    }

    // Obje deaktif olduðunda durumu sýfýrlayalým ki karýþýklýk olmasýn
    private void OnDisable()
    {
        topIcerdeMi = false;
    }
}