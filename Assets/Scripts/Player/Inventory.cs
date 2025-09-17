using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Inventory
{
    [SerializeField] private int _limiteInventory;
    [SerializeField] private List<GameObject> _inventory = new List<GameObject>();
    [SerializeField] private GameObject _element0;
    [SerializeField] public int _weaponSelected; public int WeaponSelected { get { return _weaponSelected; } }
    [SerializeField] public int _lastWeaponSelected; public int LastWeaponSelected { get { return _lastWeaponSelected; } }

    public Inventory(int limiteInventory, GameObject element0)
    {
        _limiteInventory = limiteInventory;
        _element0 = element0;
        AddWeapon(element0);
        _weaponSelected = 0;
    }

    public void AddWeapon(GameObject weapon)
    {
        if (_inventory.Count >= _limiteInventory)
        {
            Debug.Log("Inventario lleno");
        }
        if (!_inventory.Contains(weapon))
        {
            _inventory.Add(weapon);
            SelectWeapon(_inventory.Count - 1);
            _weaponSelected = _inventory.Count - 1;
        } else
        {
            Debug.Log("La arma ya esta en el inventario");
        }
    }

    public void ReAddModule(GameObject module)
    {
        if (!_inventory.Contains(module))
        {
            _inventory.Add(module);
            module.SetActive(false); // Se activará cuando se seleccione
        }
    }

    public bool ContainsModule(GameObject module)
    {
        return _inventory.Contains(module);
    }
    public void RemoveWeapon(GameObject weapon)
    {
        if (weapon == _element0) return;

        int index = _inventory.IndexOf(weapon);
        if (index != -1)
        {
            // 1. Obtener el componente Weapon
            Weapon weaponComp = weapon.GetComponent<Weapon>();

            // 2. Llamar al método de limpieza específica
            if (weaponComp != null)
            {
                weaponComp.OnRemovedFromInventory();
            }

            // 3. Resto de la lógica (estado visual, selección, etc.)
            bool wasSelected = (_weaponSelected == index);
            _inventory.RemoveAt(index);

            if (wasSelected)
            {
                _weaponSelected = 0;
            }
            else if (_weaponSelected > index)
            {
                _weaponSelected--;
            }

           
        }
    }

    // Nuevo método para actualizar la selección
    public void UpdateWeaponSelection()
    {
        // Verificar que la selección actual sea válida
        if (_weaponSelected >= _inventory.Count)
        {
            _weaponSelected = _inventory.Count - 1;
        }

        // Reactivar solo la selección actual
        //for (int i = 0; i < _inventory.Count; i++)
        //{
        //    GameObject weaponObj = _inventory[i];
        //    Weapon weapon = weaponObj.GetComponent<Weapon>();

        //    if (weapon != null)
        //    {
        //        bool isActive = (i == _weaponSelected);
        //        weaponObj.SetActive(isActive);

        //        if (isActive)
        //        {
        //            weapon.MyBodyFBX.SetActive(true);
        //        }
        //        else
        //        {
        //            weapon.MyBodyFBX.SetActive(false);
        //        }
        //    }
        //}
    }

    public GameObject SelectWeapon(int index)
    {
        // Validar índice
        if (index < 0 || index >= _inventory.Count)
        {
            index = 0;  // Forzar selección del módulo base si no es válido
        }

        // Desactivar arma actual
        if (_inventory[_weaponSelected] != null)
        {
            _inventory[_weaponSelected].SetActive(false);
            Weapon currentWeapon = _inventory[_weaponSelected].GetComponent<Weapon>();
            //if (currentWeapon != null) currentWeapon.MyBodyFBX.SetActive(false);
        }

        // Actualizar índice y activar nueva arma
        _weaponSelected = index;
        GameObject selectedWeapon = _inventory[_weaponSelected];
        selectedWeapon.SetActive(true);

        Weapon weaponComp = selectedWeapon.GetComponent<Weapon>();
        //if (weaponComp != null)
        //{
        //    weaponComp.MyBodyFBX.SetActive(true);
        //}

        return selectedWeapon;
    }

    //método para obtener un módulo por índice
    public GameObject GetModuleAtIndex(int index)
    {
        if (index < 0 || index >= _inventory.Count) return null;
        return _inventory[index];
    }
    public Animator MyCurrentAnimator()
    {
        return _inventory[_weaponSelected].GetComponent<Weapon>().MyAnimator;
    }

    public int MyItemsCount()
    {
        return _inventory.Count;
    }
}
