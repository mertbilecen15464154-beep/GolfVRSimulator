using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BotVurusYolu))]
public class BotVurusYoluEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Varsayýlan Inspector görünümünü çiz (Listeler vs. görünsün)
        DrawDefaultInspector();

        BotVurusYolu script = (BotVurusYolu)target;

        GUILayout.Space(15); // Biraz boþluk býrak

        // Butonu oluþtur
        GUI.backgroundColor = Color.green; // Buton yeþil olsun
        if (GUILayout.Button("Vuruþ Yolu Oluþtur (+)", GUILayout.Height(35)))
        {
            script.YeniVurusYoluOlustur();
        }
        GUI.backgroundColor = Color.white;
    }
}