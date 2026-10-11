using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ClubSafetySwitch : MonoBehaviour
{
    [Header("Referanslar")]
    public InputActionReference snapTurnAction;
    public GameObject clubHead;
    public GameObject clubHeadTransparent;
    public Rigidbody topRigidbody;

    [Header("Zaman Ayarlarý (Saniye)")]
    public float snapTurnBeklemeSuresi = 1.0f;
    public float vurusSonrasiPasiflesmeGecikmesi = 0.3f;
    public float cikisSonrasiEkstraBekleme = 0.5f;

    [Header("Eþik Deðerleri")]
    public float analogEsigi = 0.1f;
    public float topDurmaEsigi = 0.05f;

    private GhostTrigger ghostTrigger;
    private bool isSafetyActive = false;

    void Awake()
    {
        ghostTrigger = clubHeadTransparent.GetComponent<GhostTrigger>();

        if (topRigidbody == null)
        {
            GameObject top = GameObject.FindGameObjectWithTag("GolfBall");
            if (top != null) topRigidbody = top.GetComponent<Rigidbody>();
        }
    }

    void Update()
    {
        Vector2 inputVector = snapTurnAction.action.ReadValue<Vector2>();
        if (Mathf.Abs(inputVector.x) > analogEsigi && !isSafetyActive)
        {
            StartCoroutine(SwapHeadsRoutine(0f));
        }

        if (!isSafetyActive && TopHareketEdiyorMu())
        {
            StartCoroutine(SwapHeadsRoutine(vurusSonrasiPasiflesmeGecikmesi));
        }
    }

    bool TopHareketEdiyorMu()
    {
        if (topRigidbody == null) return false;

#if UNITY_2023_1_OR_NEWER
        return topRigidbody.linearVelocity.magnitude > topDurmaEsigi;
#else
            return topRigidbody.velocity.magnitude > topDurmaEsigi;
#endif
    }

    IEnumerator SwapHeadsRoutine(float baslangicGecikmesi)
    {
        isSafetyActive = true;

        if (baslangicGecikmesi > 0)
        {
            yield return new WaitForSeconds(baslangicGecikmesi);
        }

        clubHead.SetActive(false);
        clubHeadTransparent.SetActive(true);

        yield return new WaitForSeconds(snapTurnBeklemeSuresi);

        // --- GÜVENLÝ AKTÝFLEÞTÝRME DÖNGÜSÜ ---
        bool guvenliMi = false;

        while (!guvenliMi)
        {
            // 1. Önce topun durmasýný ve sopanýn topun içinden çýkmasýný bekle
            while (ghostTrigger.topIcerdeMi || TopHareketEdiyorMu())
            {
                yield return null;
            }

            // 2. Top durdu ve sopa dýþarýda. Þimdi 0.5 saniye bekle.
            yield return new WaitForSeconds(cikisSonrasiEkstraBekleme);

            // 3. KRÝTÝK KONTROL: 0.5 saniye sonra hala dýþarýda mýyýz?
            // Eðer oyuncu bu 0.5sn içinde sopayý tekrar topun içine soktuysa 
            // 'guvenliMi' false kalacak ve döngü en baþa dönecek.
            if (!ghostTrigger.topIcerdeMi && !TopHareketEdiyorMu())
            {
                guvenliMi = true;
            }           
        }

        // --- HER ÞEY KESÝN OLARAK GÜVENLÝ ---
        clubHeadTransparent.SetActive(false);
        clubHead.SetActive(true);

        isSafetyActive = false;
    }
}