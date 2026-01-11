using Oculus.Interaction;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Player Hands")]
    public Transform leftHand;  // Arrume o transform da mão esquerda aqui
    public Transform rightHand; // Arrume o transform da mão direita aqui

    [Header("Configs")]
    public LayerMask enemyLayer;

    // Armas atualmente equipadas
    private GameObject currentLeftWeapon;
    private GameObject currentRightWeapon;
    private List<Weapon> activeWeapons = new List<Weapon>();

    public static WeaponManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void EquipWeapon(GameObject weaponPrefab, string handSlot = "RightHand")
    {
        if (weaponPrefab == null) return;

        Transform targetHand = handSlot == "LeftHand" ? leftHand : rightHand;
        GameObject currentWeapon = handSlot == "LeftHand" ? currentLeftWeapon : currentRightWeapon;

        // Remover arma atual se existir
        if (currentWeapon != null)
        {
            RemoveWeapon(handSlot);
        }

        // Instanciar nova arma
        GameObject newWeapon = Instantiate(weaponPrefab, targetHand.position, targetHand.rotation);
        newWeapon.transform.SetParent(targetHand);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;

        // Adicionar/obter componente WeaponBase
        WeaponBase weaponComponent = newWeapon.GetComponent<WeaponBase>();
        if (weaponComponent == null)
        {
            Debug.LogError("Prefab de arma não tem componente WeaponBase!");
            Destroy(newWeapon);
            return;
        }

        // Inicializar arma
        weaponComponent.Initialize(enemyLayer);

        // Aplicar efeito especial da arma
        if (Player.instance != null)
        {
            weaponComponent.ApplyWeaponEffect(Player.instance);
        }

        activeWeapons.Add(weaponComponent);

        // Atualizar referência
        if (handSlot == "LeftHand")
            currentLeftWeapon = newWeapon;
        else
            currentRightWeapon = newWeapon;

        Debug.Log($"Arma {weaponComponent.weaponName} equipada na mão {handSlot}");
    }

    public void RemoveWeapon(string handSlot)
    {
        GameObject weaponToRemove = handSlot == "LeftHand" ? currentLeftWeapon : currentRightWeapon;

        if (weaponToRemove != null)
        {
            WeaponBase weaponComponent = weaponToRemove.GetComponent<WeaponBase>();
            if (weaponComponent != null)
            {
                // Remover efeito especial
                if (Player.instance != null)
                {
                    weaponComponent.RemoveWeaponEffect(Player.instance);
                }

                activeWeapons.Remove(weaponComponent);
            }

            Destroy(weaponToRemove);

            if (handSlot == "LeftHand")
                currentLeftWeapon = null;
            else
                currentRightWeapon = null;
        }
    }

    public int GetTotalWeaponDamage()
    {
        int totalDamage = 0;
        foreach (WeaponBase weapon in activeWeapons)
        {
            totalDamage += weapon.CurrentDamage;
        }
        return totalDamage;
    }

    public void UpdateAllWeapons()
    {
        foreach (WeaponBase weapon in activeWeapons)
        {
            // Atualizar referência ao jogador
            if (weapon != null)
            {
                // Qualquer atualização necessária
            }
        }
    }

    public void ClearAllWeapons()
    {
        if (currentLeftWeapon != null)
            RemoveWeapon("LeftHand");

        if (currentRightWeapon != null)
            RemoveWeapon("RightHand");
    }

    // Atualizar danos baseados nos stats do jogador
    public void UpdateWeaponDamage(float multiplier)
    {
        foreach (Weapon weapon in activeWeapons)
        {
            weapon.UpdateDamageMultiplier(multiplier);
        }
    }
}
