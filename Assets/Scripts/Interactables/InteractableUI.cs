using UnityEngine;

public class InteractableUI : MonoBehaviour
{
    [Header("Rangos de proximidad")]
    [Tooltip("Distancia en la que aparece la UI.")]
    public float uiRadius = 3f;
    [Tooltip("Distancia en la que comienzan las partículas.")]
    public float particleRadius = 8f;
    [Tooltip("Offset adicional para apagar partículas (en unidades).")]
    public float particleOffset = 2f;

    [Header("Teclas de interacción")]
    [Tooltip("Tecla para acciones/lore (E).")]
    public KeyCode interactKey = KeyCode.E;
    [Tooltip("Tecla para pickup/levitar (R).")]
    public KeyCode pickupKey = KeyCode.R;

    [Header("Visuales")]
    [Tooltip("Prefab de ParticleSystem.")]
    public ParticleSystem ParticlesPrefab;
    private ParticleSystem _particles;
    [Tooltip("Panel UI (\"Presiona E\").")]
    public GameObject InteractuableUI;

    [Header("Lore (solo layer “Lore”)")]
    [Tooltip("Asignar solo en objetos de layer “Lore”.")]
    public GameObject lorePanel;

    Transform _player;
    bool _uiInRange;
    bool _loreOpen;
    bool _consumed;
    bool _isLoreObject;
    int _loreLayer;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;
        _loreLayer = LayerMask.NameToLayer("Lore");
        _isLoreObject = (gameObject.layer == _loreLayer);

        // Desactivar al inicio
        if (InteractuableUI) InteractuableUI.SetActive(false);
        if (_isLoreObject && lorePanel) lorePanel.SetActive(false);

        // Instanciar partículas
        if (ParticlesPrefab)
        {
            _particles = Instantiate(
                ParticlesPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );
            _particles.Stop();
        }
    }

    void Update()
    {
        if (_player == null) return;

        float dist = Vector3.Distance(_player.position, transform.position);
        float stopDist = uiRadius + particleOffset;

        // --- TOGGLE LORE FIRST (always allowed for Lore objects)
        if (_isLoreObject && _uiInRange && Input.GetKeyDown(interactKey))
        {
            ToggleLore();
            return; // skip consumption or other logic this frame
        }

        // --- CONSUME on E (non-lore) or R
        if (_uiInRange &&
            (_isLoreObject == false && Input.GetKeyDown(interactKey)
             || Input.GetKeyDown(pickupKey)))
        {
            Consume();
            return;
        }

        if (_consumed)
            return;

        // --- PARTÍCULAS en (stopDist, particleRadius]
        if (_particles)
        {
            if (dist > stopDist && dist <= particleRadius)
            {
                if (!_particles.isPlaying) _particles.Play();
            }
            else if (_particles.isPlaying)
            {
                _particles.Stop();
            }
        }

        // --- UI prompt dentro de uiRadius
        bool shouldShowUI = dist <= uiRadius;
        if (!_uiInRange && shouldShowUI) EnterUI();
        else if (_uiInRange && !shouldShowUI) ExitUI();
    }

    void Consume()
    {
        _consumed = true;
        // Ocultar todo menos lore
        if (InteractuableUI) InteractuableUI.SetActive(false);
        if (_particles && _particles.isPlaying) _particles.Stop();
    }

    void EnterUI()
    {
        _uiInRange = true;
        if (InteractuableUI) InteractuableUI.SetActive(true);
    }

    void ExitUI()
    {
        _uiInRange = false;
        if (InteractuableUI) InteractuableUI.SetActive(false);
        if (_isLoreObject && _loreOpen) CloseLore();
    }

    void ToggleLore()
    {
        // lorePanel debe existir si es objeto Lore
        _loreOpen = !_loreOpen;
        lorePanel.SetActive(_loreOpen);
        InteractuableUI.SetActive(!_loreOpen);
    }

    void CloseLore()
    {
        _loreOpen = false;
        lorePanel.SetActive(false);
        InteractuableUI.SetActive(true);
    }
}
