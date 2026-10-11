using UnityEngine;
using System.Collections.Generic;

public class BotPath : MonoBehaviour
{
    public List<Transform> shotPoints = new List<Transform>();

    private void OnDrawGizmos()
    {
        if (shotPoints == null || shotPoints.Count == 0) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < shotPoints.Count; i++)
        {
            if (shotPoints[i] == null) continue;

            // Noktaya bir küre çiz
            Gizmos.DrawWireSphere(shotPoints[i].position, 0.15f);

            // Bir sonraki noktaya çizgi çek
            if (i < shotPoints.Count - 1 && shotPoints[i + 1] != null)
            {
                Gizmos.DrawLine(shotPoints[i].position, shotPoints[i + 1].position);

                // Ok yönü göster (Küçük bir detay)
                Vector3 direction = (shotPoints[i + 1].position - shotPoints[i].position).normalized;
                Gizmos.DrawRay(shotPoints[i].position, direction * 0.5f);
            }
        }
    }
}