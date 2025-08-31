using UnityEngine;

[RequireComponent(typeof(Light))]
public class RainbowLight : MonoBehaviour
{
    [Tooltip("Velocidad a la que cambia el color (vueltas completas de 0 a 1 por segundo).")]
    public float hueSpeed = 0.2f;
    [Tooltip("Saturación del color (0 = gris, 1 = color puro).")]
    [Range(0f, 1f)]
    public float saturation = 1f;
    [Tooltip("Valor/brillo del color (0 = negro, 1 = brillo máximo).")]
    [Range(0f, 1f)]
    public float value = 1f;

    private Light _light;
    private float _hue;

    void Awake()
    {
        _light = GetComponent<Light>();
    }

    void Update()
    {
        // Incrementamos el hue continuamente
        _hue += hueSpeed * Time.deltaTime;
        if (_hue > 1f) _hue -= 1f;

        // Convertimos HSV a RGB
        Color c = Color.HSVToRGB(_hue, saturation, value);
        _light.color = c;
    }
}
