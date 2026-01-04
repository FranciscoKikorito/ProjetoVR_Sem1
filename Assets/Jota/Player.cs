using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    [SerializeField]
    private GameObject[] weapons;

    public int augmentCount;
    public int killCount;

    [SerializeField]
    private Stats playerBaseStats;

    public float currentHP;

    public static Player instance;

    private void Start()
    {
        currentHP = playerBaseStats.health;
    }

    private void Update()
    {
        
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP <= 0) 
        { 
            OnPlayerDeath();
        }
    }

    public void OnPlayerDeath()
    {

    }
}
