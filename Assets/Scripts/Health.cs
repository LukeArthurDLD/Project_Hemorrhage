using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth = 100;

    public SliderBar healthbar; //healthbar
    public bool showAtFull = false;

    //health Regeneration
    public bool regenerates = false;
    [Range(0, 1)]
    public float regenRate = 0f;
    public float regenDelay = 2f;
    private Coroutine regen;

    [System.NonSerialized]
    public bool isDead = false;
    private void Awake()
    {
        if (healthbar == null)
            healthbar = GetComponentInChildren<SliderBar>();
    }
    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }
    private void Update()
    {
        if (healthbar)
        {
            if (!showAtFull && currentHealth == maxHealth)
            {
                healthbar.gameObject.SetActive(false);
            }
            else
            {
                healthbar.gameObject.SetActive(true);
            }
        }

    }
    private IEnumerator RegenHP()
    {
        yield return new WaitForSeconds(regenDelay);

        while (currentHealth < maxHealth && !isDead)
        {
            currentHealth += maxHealth / 100;
            yield return new WaitForSeconds(regenRate);

            UpdateUI();
        }        
        regen = null;
    }
    public void Heal(int heal)
    {
        if (currentHealth < maxHealth && !isDead)
        {
            currentHealth += heal;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
            UpdateUI();
        }
    }
    public void TakeDamage(int damage)
    {
        if (!isDead)
        {
            //damage is done            
            currentHealth -= (damage);

            //health is capped
            if (currentHealth < 0)
                currentHealth = 0;

            //destroys if dies
            if (currentHealth == 0)
            {
                isDead = true;
                OnDeath();
                return;
            }

            //regen
            if (regenerates && currentHealth != maxHealth)
            {
                if (regen != null)
                    StopCoroutine(regen);

                regen = StartCoroutine(RegenHP());
            }
            UpdateUI();
        }
    }
    void UpdateUI()
    {
        if (healthbar)
        {
            healthbar.SetMaxValue(maxHealth);
            healthbar.SetValue(currentHealth);
        }
    }
    void OnDeath()
    {
        UpdateUI();
        Destroy(gameObject);
    }

    public void SetHealth(int health)
    {
        maxHealth = health;
        currentHealth = health;
        UpdateUI();
    }
}
