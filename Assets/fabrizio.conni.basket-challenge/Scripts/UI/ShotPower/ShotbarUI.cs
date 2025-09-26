using UnityEngine;
using UnityEngine.UI;

public class ShotbarUI : MonoBehaviour
{
    [SerializeField] private Image shotBar;       // la barra verticale
    [SerializeField] private RectTransform greenZone; // immagine verde
    [SerializeField] private RectTransform purpleZone;

    public float SetPurpleZone(float vPerfect, float tolerance)
    {
        RectTransform barRect = (RectTransform)shotBar.transform;
        float barWidth = barRect.rect.width;

        // Normalizzazione del valore perfetto
        float normalized = Mathf.Clamp01(vPerfect / 10f);

        // Calcolo posizione in pixel
        float xPos = Mathf.Lerp(0, barWidth, normalized);

        // Offset pivot
        float pivotOffset = barWidth * barRect.pivot.x;

        // Spostamento verso destra
        float correctedX = xPos - pivotOffset + (0.2f * barWidth);

        // Posiziona la zona
        purpleZone.localPosition = new Vector3(
            correctedX,
            purpleZone.localPosition.y,
            purpleZone.localPosition.z
        );

        // 🔥 Ricalcolo normalizzato in base alla posizione reale della zona viola
        float normalizedCorrected = Mathf.InverseLerp(
            -pivotOffset,
            barWidth - pivotOffset,
            correctedX
        );

        Debug.Log($"PurpleZone Normalizzato: {normalizedCorrected}");

        return normalizedCorrected;
    }
    // Imposta la zona verde in base alla velocità perfetta
    public float SetGreenZone(float vPerfect, float tolerance)
    {
        // Larghezza della barra
        float barWidth = ((RectTransform)shotBar.transform).rect.width;

        // Posizione orizzontale corrispondente al valore perfetto
        float xPos = Mathf.Lerp(0, barWidth, vPerfect/10);

        // Correggi per pivot (se sta al centro)
        float pivotOffset = ((RectTransform)shotBar.transform).rect.width * ((RectTransform)shotBar.transform).pivot.x;

        float correctedX = xPos - pivotOffset;

        Debug.Log($"Posizione X corretta: {correctedX}");

        // Aggiorna dimensione e posizione
        greenZone.localPosition = new Vector3(
            correctedX,
            greenZone.localPosition.y,
            greenZone.localPosition.z
        );
        float normalized = Mathf.Clamp01(vPerfect / 10f);

        return normalized;
    }

    // Aggiorna il cursore in base all’input
    public void UpdateCursor(float vInput)
    {
        shotBar.fillAmount = vInput;
    }
}
