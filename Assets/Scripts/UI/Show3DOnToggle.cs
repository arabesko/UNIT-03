using UnityEngine;
using System.Collections;

public class Show3DOnToggle : MonoBehaviour
{
    [Header("Referencias a Objetos 3D (en Canvas)")]
    [Tooltip("Arrastra aquí tus GameObjects 3D que están en el canvas")]
    public GameObject[] ui3DObjects;

    // Arrays para guardar posiciones y rotaciones locales originales
    private Vector3[] originalLocalPos;
    private Quaternion[] originalLocalRot;

    void Awake()
    {
        if (ui3DObjects != null && ui3DObjects.Length > 0)
        {
            int count = ui3DObjects.Length;
            originalLocalPos = new Vector3[count];
            originalLocalRot = new Quaternion[count];

            for (int i = 0; i < count; i++)
            {
                var obj = ui3DObjects[i];
                if (obj != null)
                {
                    // Guardar estado local
                    originalLocalPos[i] = obj.transform.localPosition;
                    originalLocalRot[i] = obj.transform.localRotation;
                    // Desactivar al inicio
                    obj.SetActive(false);
                }
            }
        }
    }

    void OnEnable()
    {
        // Activar todos los objetos y, si quieres, iniciar animación aquí
        if (ui3DObjects != null)
        {
            for (int i = 0; i < ui3DObjects.Length; i++)
            {
                var obj = ui3DObjects[i];
                if (obj != null)
                {
                    obj.SetActive(true);
                    // Si deseas reiniciar posición/rotación, descomenta:
                    // obj.transform.localPosition = originalLocalPos[i];
                    // obj.transform.localRotation = originalLocalRot[i];
                }
            }
        }
    }

    void OnDisable()
    {
        // Desactivar todos los objetos
        if (ui3DObjects != null)
        {
            for (int i = 0; i < ui3DObjects.Length; i++)
            {
                var obj = ui3DObjects[i];
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }
}
