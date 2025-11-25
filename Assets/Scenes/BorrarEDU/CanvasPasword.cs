using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CanvasPasword : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textPasword; //Es la UI que muestra la contraseña
    [SerializeField] private string _textPaswordAdd; //Es la contraseña digitada con los botones
    [SerializeField] private string _pasword; //Es la contraseña

    [SerializeField] private AudioSource _audioSource;
    
    [SerializeField] private AudioClip _audioPaswordNO;
    [SerializeField] private AudioClip _audioKeyPress;

    [SerializeField] private List<Button> _myButtons;

    [SerializeField] KeyOpenDoor _keyOpenDoor;

    public void WriteNumeber(int number)
    {
        _audioSource.PlayOneShot(_audioKeyPress);
        _textPaswordAdd = _textPaswordAdd.Trim() + number;
        if (_textPaswordAdd.Trim().Length >= 4)
        {
            _textPasword.text = _textPaswordAdd;
            MyButtonsActivate(false);
            if (_textPaswordAdd.Trim() == _pasword.Trim())
            {
                //Clave correcta
                
                _keyOpenDoor._isCorrectKey = true;
            }
            else
            {
                //Clave incorrecta
                _audioSource.PlayOneShot(_audioPaswordNO);
                _textPaswordAdd = "";
                _textPasword.text = "";
                StartCoroutine(TimeToErasePasw());
            }
        }
        else
        {
            _textPasword.text = _textPaswordAdd;
        }
    }

    public void ExitPasw()
    {
        _keyOpenDoor.HideKeyBoard();
    }

    public IEnumerator TimeToErasePasw()
    {
        print("corrutina erasespaw");
        yield return new WaitForSeconds(1.5f);
        _textPaswordAdd = "";
        _textPasword.text = "";
        MyButtonsActivate(true);
        _keyOpenDoor.HideKeyBoard();
    }

    public void MyButtonsActivate(bool isActivate)
    {
        foreach (Button item in _myButtons)
        {
            item.interactable = isActivate;
        }
    }
}
