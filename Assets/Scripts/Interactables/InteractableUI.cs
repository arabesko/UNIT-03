using UnityEngine;

public class InteractableUI : MonoBehaviour
{
    [Header("Rangos de proximidad")]
    public float uiRadius = 3f;
    public float particleRadius = 8f;
    public float particleOffset = 2f;

    [Header("Teclas de interacción")]
    public KeyCode interactKey = KeyCode.E;
    public KeyCode pickupKey = KeyCode.R;

    [Header("Visuales")]
    public ParticleSystem ParticlesPrefab;
    private ParticleSystem _particles;
    public GameObject InteractuableUI;

    [Header("Lore (solo layer “Lore”)")]
    public GameObject lorePanel;

    Transform _player;
    PlayerMovement _playerMovement;
    bool _uiInRange;
    bool _loreOpen;
    bool _consumed;          // uso: objeto consumido/permanente
    bool _isLoreObject;
    int _loreLayer;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (_player != null)
            _playerMovement = _player.GetComponent<PlayerMovement>();

        _loreLayer = LayerMask.NameToLayer("Lore");
        _isLoreObject = (gameObject.layer == _loreLayer);

        if (InteractuableUI) InteractuableUI.SetActive(false);
        if (_isLoreObject && lorePanel) lorePanel.SetActive(false);

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
        if (_isLoreObject && dist <= uiRadius && Input.GetKeyDown(interactKey))
        {
            ToggleLore();
            return;
        }

        // IMPORTANT: removimos la llamada a Consume() aquí.
        // No queremos marcar como "consumido" cuando solo se levita (R).
        // El Player debe llamar a MarkAsConsumedPermanent() cuando haya una recolección definitiva.

        if (_consumed)
            return;

        // --- PARTICULAS
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

        // --- Lógica mostrar/ocultar prompt
        bool shouldShowUI = dist <= uiRadius;

        // Si el player tiene este mismo objeto levitado => ocultar su UI
        if (_playerMovement != null && _playerMovement.ElementLevitated == this.gameObject)
        {
            shouldShowUI = false;
        }

        // EXCEPCIÓN: si el player aprieta E y está en rango, forzamos mostrar la UI.
        if (Input.GetKeyDown(interactKey) && dist <= uiRadius)
        {
            shouldShowUI = true;
            if (InteractuableUI) InteractuableUI.SetActive(true);
        }

        if (!_uiInRange && shouldShowUI) EnterUI();
        else if (_uiInRange && !shouldShowUI) ExitUI();
    }

    // Llamar desde Player cuando se quiera marcar como consumido/permanente
    public void MarkAsConsumedPermanent()
    {
        _consumed = true;
        if (InteractuableUI) InteractuableUI.SetActive(false);
        if (_particles && _particles.isPlaying) _particles.Stop();
    }

    // Si querés volver a "reactivar" (útil para debugging)
    public void ResetConsumed()
    {
        _consumed = false;
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
