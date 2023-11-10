using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile Bullet", menuName = "Bullets/Projectile Bullet")]
public class ProjectileBullet : Bullet
{
    [Header("Projectile")]
    public Projectile projectile;
    //forces
    [Header("Launch Properties")]
    public float forwardForce;
    public float upwardForce;
    public float torque;

    [Header("Physics")]
    [Range(0f, 1f)]
    public float bounciness;
    public bool useGravity;

    [Header("Lifetime and Collisions")]
    public float maxLifetime;
    public int minCollisions;
    public int maxCollisions;
    public bool explodeOnTouch;


    public override void OnFire(Transform weaponOrigin, Transform raycastOrigin)
    {
        //find hit position     
        ray.origin = raycastOrigin.position;
        ray.direction = raycastOrigin.forward;

        //checks if ray hits
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(75);

        //instatiates projectile
        GameObject currentProjectile = Instantiate(projectile.gameObject, weaponOrigin.position, Quaternion.identity);
        currentProjectile.transform.forward = raycastOrigin.forward.normalized;

        AddProjectileProperties(currentProjectile);

        //adds force to projectile
        currentProjectile.GetComponent<Rigidbody>().AddForce(raycastOrigin.forward.normalized * forwardForce, ForceMode.Impulse);
        currentProjectile.GetComponent<Rigidbody>().AddForce(weaponOrigin.transform.up * upwardForce, ForceMode.Impulse);
        //torque
        float random = Random.Range(-torque, torque);
        currentProjectile.GetComponent<Rigidbody>().AddTorque(new Vector3(random, random, random) * 10);

    }
    void AddProjectileProperties(GameObject projectile)
    {
        Projectile p = projectile.GetComponent<Projectile>();

        p.damage = damage;

        //explosion
        p.explosionDamage = explosionDamage;
        p.explosionRange = explosionRange;
        p.explosionForce = explosionForce;
        p.explosionEffect = explosionEffect;

        p.useGravity = useGravity;
        p.bounciness = bounciness;

        //lifetime
        p.maxLifetime = maxLifetime;
        p.maxCollisions = maxCollisions;
        p.minCollisions = minCollisions;
        p.explodeOnTouch = explodeOnTouch;
    }
}
