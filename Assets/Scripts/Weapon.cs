using System.Collections;
using System;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Identity")]
    public string weaponName = "Default";
    public string weaponDescription = "Default";

    public Bullet[] bullet;
    int bulletIndex = 0;
    [System.NonSerialized]
    public WeaponManager weaponManager;

    [Header("Firerate")]
    public float fireRate = 2.5f; //bullets per second
    private float nextFire = 0f;

    [Header("Accuracy")]
    public float spreadPerShot = 1.0f;
    public float minimumSpread = 0f;
    public float maximumSpread = 10f;
    public enum FiringMode { SemiAuto, FullAuto, Burst, Charge };
    public FiringMode firingMode;

    [Header("Burst")]
    public int bulletsPerBurst = 3;
    public float timeBetweenBurst = 0.2f;
    private int bulletsShot = 0;

    [Header("Charge")]
    public float minChargeTime = 1f;
    public float maxChargeTime = 0f;
    private float timeCharged = 0f;

    [Header("State")]
    public enum WeaponState { Neutral, Reloading, Firing, Melee, Dropped };
    public WeaponState weaponState = WeaponState.Neutral;

    [Header("Effects")]
    public Rigidbody shell;
    public Transform shellEject;
    public GameObject muzzleFlash;

    //raycast
    Ray ray;
    RaycastHit hit;
    [Header("Raycast")]
    public Transform raycastOrigin;
    public Transform weaponOrigin;

    [Header("UI")]
    public SliderBar ammoDisplay;
    public Vector3 offset;

    private void Awake()
    {
        Invoke(nameof(SetUp), 0.1f);
    }
    public void SetUp()
    {
        weaponManager = GetComponentInParent<WeaponManager>();

        //handle if dropped or not
        if (weaponManager) 
        {
            weaponState = WeaponState.Neutral;
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.None;
            GetComponent<Collider>().isTrigger = true;
        }
        else
        {
            weaponState = WeaponState.Dropped;
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            GetComponent<Collider>().isTrigger = false;
        }

        if (raycastOrigin == null)
            raycastOrigin = weaponOrigin;
        
        //UI
        SetUI();
    }
    private void Update()
    {
        if(magType == MagazineType.Battery && currentHeat > 0 ) // if battery
        {
            CoolWeapon();
        }
        if (firingMode == FiringMode.Charge && maxChargeTime != 0 && timeCharged == maxChargeTime) // if charge time exceeds max charge time
        {
            timeCharged = 0;
            OnFire();
        }
    }
    public void MyInput()
    {
        if (weaponState == WeaponState.Neutral || weaponState == WeaponState.Firing)
        {
            if ((Input.GetButton("Fire1"))) // Hold input
            {
                if (firingMode == FiringMode.FullAuto) // full auto
                    OnFire();
                else if (firingMode == FiringMode.Charge) // charge
                    timeCharged += Time.deltaTime;
            }
            if (Input.GetButtonDown("Fire1")) // tap input
            {
                if (firingMode == FiringMode.SemiAuto) // semi auto
                    OnFire();
            }
            if (Input.GetButtonUp("Fire1")) // release input
            {
                if (firingMode == FiringMode.Charge) // charge release
                {
                    if (timeCharged >= minChargeTime) // if charged
                    {
                        OnFire();                        
                    }
                    else if (bullet.Length > 1)
                    {
                        bulletIndex++;
                        OnFire();
                        bulletIndex--;
                    }
                    timeCharged = 0;
                }
            }
        } 
    }
    public void OnFire()
    {
        if (Time.time >= nextFire)
        {
            Debug.Log("Trigger Pulled");
            nextFire = Time.time + 1f / fireRate; //manage firerate

            if (firingMode == FiringMode.Burst) //handles burst
                bulletsShot = bulletsPerBurst;       
            
            //add ammo check with new ammo system
            Shoot();
        }
    }
    void Shoot()
    {
        //ammo management
        ManageAmmo();
        UpdateUI();

        //play effects
        if (muzzleFlash)
        {
            GameObject flash = Instantiate(muzzleFlash, weaponOrigin);
            Destroy(flash, 0.2f);
        }

        //Fire bullet        
        bullet[bulletIndex].OnFire(weaponOrigin, raycastOrigin);

        //shell eject
        if (shell && shellEject)
            EjectShell();

        //battery
        if (magType == MagazineType.Battery)
        {
            currentHeat += heatPerShot;

            if (currentHeat >= maxHeat)
                OverheatWeapon();

            UpdateUI();
        }

        //burst
        if (firingMode == FiringMode.Burst)
        {
            bulletsShot--;
            if( bulletsShot > 0 && currentMagAmmo != 0)
            {
                Invoke(nameof(Shoot), timeBetweenBurst);
            }
        }
    }
    void ManageAmmo()
    {
        int ammoCost = bullet[bulletIndex].ammoCost;

        if(magType == MagazineType.Battery)        
            currentReserveAmmo -= ammoCost;        
        else
            currentMagAmmo -= ammoCost;
    }
    void EjectShell()
    {
        Rigidbody newShell = Instantiate(shell, shellEject.position, weaponOrigin.rotation) as Rigidbody;
        newShell.AddForce(shellEject.forward * UnityEngine.Random.Range(150f, 200f) + weaponOrigin.forward * UnityEngine.Random.Range(-10f, 10f));

        float random = UnityEngine.Random.Range(-1f, 1f);
        newShell.AddTorque(new Vector3(random, random, random) * 10);

        Destroy(newShell.gameObject, 15f);
    }
    void StartReload()
    {
        Debug.Log("Reloading: " + reloadSpeed);
        Invoke(nameof(Reload), reloadSpeed);
        
        weaponState = WeaponState.Reloading;
    }
    void Reload()
    {        
        if (magType == MagazineType.Tube) // reload one bullet at a time
        {
            if (currentReserveAmmo > 0)
            {
                currentMagAmmo++;
                currentReserveAmmo--;
            }
        }
        else // reload entire clip at once
        {
            int ammoNeeded = maxMagAmmo - currentMagAmmo;

            if (ammoNeeded <= currentReserveAmmo)
            {
                currentMagAmmo = maxMagAmmo;
                currentReserveAmmo -= ammoNeeded;
            }
            else
            {
                currentMagAmmo += currentReserveAmmo;
                currentReserveAmmo = 0;
            }
        }

        Debug.Log("Reloaded");

        UpdateUI();
        weaponState = WeaponState.Neutral;

        if (magType == MagazineType.Tube)
        {
            if (currentMagAmmo >= maxMagAmmo)
                currentMagAmmo = maxMagAmmo;
            else
                StartReload();
        }
    }
    void OverheatWeapon()
    {        
        weaponState = WeaponState.Reloading;
        currentHeat = maxHeat;

        Invoke(nameof(StartCooling), reloadSpeed);
    }
    void StartCooling()
    {        
        weaponState = WeaponState.Reloading;
        CoolWeapon();
    }
    private void CoolWeapon()
    {
        float cool; //cool rate

        if (weaponState == WeaponState.Reloading) // enhances cool speed if venting
            cool = coolRate * 2;
        else // slows cool speed if using weapon
            cool = coolRate / 2;

        currentHeat -= (cool) * Time.deltaTime; //cool weapon

        if (currentHeat < 0)
            currentHeat = 0;
        UpdateUI();

        if (weaponState == WeaponState.Reloading) //if venting, prevents gun from being used until completely cool
        {
            if (currentHeat <= 0)
                weaponState = WeaponState.Neutral;
        }
    }
    void SetUI()
    {
        if(ammoDisplay)
        {
            if (magType == MagazineType.Battery)
                ammoDisplay.SetMaxValue((int)Math.Round(maxHeat));
            else
                ammoDisplay.SetMaxValue(maxMagAmmo);
        }

        UpdateUI();
    }
    void UpdateUI()
    {
        if (ammoDisplay)
        {
            if (magType == MagazineType.Battery)
                ammoDisplay.SetValue((int)Math.Round(currentHeat));
            else
                ammoDisplay.SetValue(currentMagAmmo);
        }
        if (weaponManager)
        {
            weaponManager.UpdateUI();
        }
    }    
}