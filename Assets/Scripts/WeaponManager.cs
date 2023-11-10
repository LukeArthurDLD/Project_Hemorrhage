using UnityEngine;
using System;

public class WeaponManager : MonoBehaviour
{
    Transform[] weapons;
    private int selectedWeapon = 0;
    private int currentWeapon = 1;

    [Header("WeaponPickup")]
    public float pickUpRange = 2f;
    public Transform Unarmed;
    public Transform closestWeapon = null;

    [Header("UI")]
    public SliderBar ammoBar;
    public SliderBar reserveBar;
    public SliderBar batteryBar;

    void Start()
    {
        weapons = new Transform[transform.childCount];
        for(int i = 0; i < weapons.Length; i++)
        {
            weapons[i] = transform.GetChild(i);
            Pickup(weapons[i]);
        }
        SelectWeapon(selectedWeapon);
        InvokeRepeating(nameof(FindWeapons), 0f, 0.5f);
    }
    void Update()
    {
        weapons[currentWeapon].GetComponent<Weapon>().MyInput();
        MyInput();
    }
    void MyInput()
    {
        // keyboard input
        for (int i = 1; i <= weapons.Length; i++)
        {
            if (Input.GetKeyDown("" + i))
            {
                selectedWeapon = i - 1;

                SelectWeapon(selectedWeapon);
            }
        }
        // scroll wheel input
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            selectedWeapon++;
            if (selectedWeapon > weapons.Length - 1) // cycles weapon to first 
                selectedWeapon = 0;

            SelectWeapon(selectedWeapon);
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f) 
        {
            selectedWeapon--;
            if (selectedWeapon < 0) // cycles weapon to first 
                selectedWeapon = weapons.Length - 1;

            SelectWeapon(selectedWeapon);
        }        
        
        if (closestWeapon != null)
        {
            float distanceToWeapon = Vector3.Distance(transform.position, closestWeapon.position);
            if (distanceToWeapon <= pickUpRange)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Drop(currentWeapon);
                    Pickup(closestWeapon);
                    SetUI();
                }                
            }
        }
    }    
    void SelectWeapon(int index)
    {
        if (index != currentWeapon)
        {
            currentWeapon = index;
                        
            for(int i = 0; i < weapons.Length; i++)
            {
                if (i == index)
                    weapons[i].gameObject.SetActive(true);
                else
                    weapons[i].gameObject.SetActive(false);
            }
            SetUI();
        }
    }
    void FindWeapons()
    {
        Collider[] weaponPickups = Physics.OverlapSphere(transform.position, pickUpRange);
        float shortestDistance = Mathf.Infinity;

        for(int i = 0; i < weaponPickups.Length; i++)
        {
            if (weaponPickups[i].GetComponent<Weapon>())
            {
                if (weaponPickups[i].GetComponent<Weapon>().weaponState != Weapon.WeaponState.Dropped)
                    continue;

                float distanceToWeapon = Vector3.Distance(transform.position, weaponPickups[i].transform.position);
                if (distanceToWeapon < shortestDistance)
                {
                    shortestDistance = distanceToWeapon;
                    closestWeapon = weaponPickups[i].transform;
                }
            }
        }       
    }
    
    void Pickup(Transform weapon)
    {
        // components
        Rigidbody weaponRB = weapon.GetComponent<Rigidbody>();
        Weapon weaponComp = weapon.GetComponent<Weapon>();

        // set position
        weapon.SetParent(transform);
        weapon.localPosition = Vector3.zero + weaponComp.offset;
        weapon.localRotation = Quaternion.Euler(Vector3.zero);
        weapon.localScale = Vector3.one;

        // ready weapon
        weaponComp.weaponState = Weapon.WeaponState.Neutral;
        weapons[currentWeapon] = weapon;        

        // handle physics
        weaponRB.isKinematic = true;
        weaponRB.interpolation = RigidbodyInterpolation.None;
        weapon.GetComponent<Collider>().isTrigger = true;

        // setup UI
        weapon.GetComponent<Weapon>().SetUp();
    }
    void Drop(int index)
    {
        Transform weapon = weapons[index];
        Rigidbody weaponRB = weapon.GetComponent<Rigidbody>();


        weapon.SetParent(null);
        weapon.GetComponent<Weapon>().weaponState = Weapon.WeaponState.Dropped;

        // handle physics
        weaponRB.isKinematic = false;
        weaponRB.interpolation = RigidbodyInterpolation.Interpolate;
        weapon.GetComponent<Collider>().isTrigger = false;

        // momentum
        weaponRB.velocity = GetComponentInParent<Rigidbody>().velocity;
        float random = UnityEngine.Random.Range(-1f, 1f);
        weaponRB.AddTorque(new Vector3(random, random, random) * 10);

    }
    public void SetUI()
    {
        Weapon weaponUI = weapons[currentWeapon].GetComponent<Weapon>();

        if (weaponUI.magType == Weapon.MagazineType.Battery) // battery
        {
            if(batteryBar)
            {
                batteryBar.gameObject.SetActive(true);
                batteryBar.SetMaxValue((int)Math.Round(weaponUI.maxHeat));
                batteryBar.SetValue((int)Math.Round(weaponUI.currentHeat));               
            }
            if (ammoBar)
                ammoBar.gameObject.SetActive(false);
        }
        else 
        {
            if (ammoBar)
            {
                ammoBar.gameObject.SetActive(true);
                ammoBar.SetMaxValue(weaponUI.maxMagAmmo);
                ammoBar.SetValue(weaponUI.currentMagAmmo);
            }
            if (batteryBar)
                batteryBar.gameObject.SetActive(false);
        }
            
        if (reserveBar) // set reserve ammo
        {
            reserveBar.SetMaxValue(weaponUI.maxReserveAmmo);
            reserveBar.SetValue(weaponUI.currentReserveAmmo);
        }
    }
    public void UpdateUI()
    {
        Weapon weaponUI = weapons[currentWeapon].GetComponent<Weapon>();

        if (weaponUI.magType == Weapon.MagazineType.Battery) // battery
        {
            if (batteryBar && currentWeapon < weapons.Length) // set heat
                batteryBar.SetValue((int)Math.Round(weaponUI.currentHeat));           
        }
        else // magazine weapon
        {
            if (ammoBar && currentWeapon < weapons.Length) // set current ammo
                ammoBar.SetValue(weaponUI.currentMagAmmo);
        }

        if (reserveBar && currentWeapon < weapons.Length) // set reserve ammo
            reserveBar.SetValue(weaponUI.currentReserveAmmo);
    
    }
}

