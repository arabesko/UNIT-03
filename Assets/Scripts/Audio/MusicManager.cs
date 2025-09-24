using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio")]
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioMixer _mixer;

    // Sliders (se buscarán por tag en runtime)
    private Slider _masterSlider;
    private Slider _musicSlider;
    private Slider _sfxSlider;

    [Header("Initial values")]
    [SerializeField] private float _initMasterVol = .5f;
    [SerializeField] private float _initMusicVol = .5f;
    [SerializeField] private float _initSFXVol = 1f;

    // Valores actuales (0..1)
    private float _masterValue;
    private float _musicValue;
    private float _sfxValue;

    private const string PREF_MASTER = "MasterVol";
    private const string PREF_MUSIC = "MusicVol";
    private const string PREF_SFX = "SFXVol";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Cargar valores guardados o iniciales
            _masterValue = PlayerPrefs.GetFloat(PREF_MASTER, _initMasterVol);
            _musicValue = PlayerPrefs.GetFloat(PREF_MUSIC, _initMusicVol);
            _sfxValue = PlayerPrefs.GetFloat(PREF_SFX, _initSFXVol);

            // Aplicar inmediatamente al mixer
            ApplyToMixer();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeSliderListeners();
    }

    private void Start()
    {
        // Intentamos conectar con los sliders ya presentes en la escena
        TryBindSlidersByTag();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Intentamos reconectar al cargar una nueva escena
        TryBindSlidersByTag();
    }

    // ---------- BINDING ----------

    // Método público que podés llamar desde tu script de pausa cuando la UI se active:
    // por ejemplo: void OnEnable() { if (MusicManager.Instance != null) MusicManager.Instance.RefreshUIBindings(); }
    public void RefreshUIBindings()
    {
        TryBindSlidersByTag();
    }

    private void TryBindSlidersByTag()
    {
        // Buscar por tags. Asegurate de que los GameObjects tengan esos tags creados y asignados.
        // FindWithTag solo encuentra objetos activos en la jerarquía.
        GameObject g;

        g = GameObject.FindWithTag("MasterSlider");
        _masterSlider = (g != null) ? g.GetComponent<Slider>() : null;

        g = GameObject.FindWithTag("MusicSlider");
        _musicSlider = (g != null) ? g.GetComponent<Slider>() : null;

        g = GameObject.FindWithTag("SFXSlider");
        _sfxSlider = (g != null) ? g.GetComponent<Slider>() : null;

        // Asignar los valores que ya tenemos guardados (sin disparar listeners todavía)
        if (_masterSlider != null) _masterSlider.value = _masterValue;
        if (_musicSlider != null) _musicSlider.value = _musicValue;
        if (_sfxSlider != null) _sfxSlider.value = _sfxValue;

        // (Re)Suscribir listeners
        SubscribeSliderListeners();
    }

    private void SubscribeSliderListeners()
    {
        UnsubscribeSliderListeners();

        if (_masterSlider != null) _masterSlider.onValueChanged.AddListener(SetMasterVolume);
        if (_musicSlider != null) _musicSlider.onValueChanged.AddListener(SetMusicVolume);
        if (_sfxSlider != null) _sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    private void UnsubscribeSliderListeners()
    {
        if (_masterSlider != null) _masterSlider.onValueChanged.RemoveListener(SetMasterVolume);
        if (_musicSlider != null) _musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
        if (_sfxSlider != null) _sfxSlider.onValueChanged.RemoveListener(SetSFXVolume);
    }

    // ---------- VOLUMEN / MIXER ----------

    private float SafeLogVolume(float value)
    {
        float v = Mathf.Clamp(value, 0.0001f, 1f);
        return Mathf.Log10(v) * 20f;
    }

    private void ApplyToMixer()
    {
        if (_mixer == null) return;
        _mixer.SetFloat("MasterVol", SafeLogVolume(_masterValue));
        _mixer.SetFloat("MusicVol", SafeLogVolume(_musicValue));
        _mixer.SetFloat("SFXVol", SafeLogVolume(_sfxValue));
    }

    public void SetMasterVolume(float value)
    {
        _masterValue = Mathf.Clamp01(value);
        if (_mixer != null) _mixer.SetFloat("MasterVol", SafeLogVolume(_masterValue));
        PlayerPrefs.SetFloat(PREF_MASTER, _masterValue);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float value)
    {
        _musicValue = Mathf.Clamp01(value);
        if (_mixer != null) _mixer.SetFloat("MusicVol", SafeLogVolume(_musicValue));
        PlayerPrefs.SetFloat(PREF_MUSIC, _musicValue);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        _sfxValue = Mathf.Clamp01(value);
        if (_mixer != null) _mixer.SetFloat("SFXVol", SafeLogVolume(_sfxValue));
        PlayerPrefs.SetFloat(PREF_SFX, _sfxValue);
        PlayerPrefs.Save();
    }

    public void PlayAudio(AudioClip clip)
    {
        if (clip == null || _source == null) return;
        if (clip == _source.clip) return;
        _source.Stop();
        _source.clip = clip;
        _source.Play();
    }

    // Getters por si los necesitás
    public float GetMasterValue() { return _masterValue; }
    public float GetMusicValue() { return _musicValue; }
    public float GetSFXValue() { return _sfxValue; }
}