using UnityEngine;
public abstract class Bullet : ScriptableObject
{
    public int damage;
    public float range = 25; //units
    public int ammoCost = 1;
    public float spread;

    //for shotgun
    public int shotsPerBullet = 1;
    [HideInInspector] public int shotsLeft;

    public enum DamageType { LowCalibre, MediumCalibre, HighCalibre, Energy, Explosive };
    public DamageType damageType = DamageType.LowCalibre;

    [Header("Explosion")]
    public int explosionDamage = 0;
    public float explosionRange = 0;
    public float explosionForce = 0;
    public GameObject explosionEffect;

    //raycast
    [HideInInspector]
    public RaycastHit hit;
    [HideInInspector]
    public Ray ray;

    public abstract void OnFire(Transform weaponOrigin, Transform raycastOrigin);
    public void Explode(Vector3 position)
    {
        if (explosionEffect)
        {
            GameObject explosion = Instantiate(explosionEffect, position, Quaternion.identity);
            Destroy(explosion, 1);
        }

        Collider[] targets = Physics.OverlapSphere(position, explosionRange);
        foreach (Collider nearbyObject in targets)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddExplosionForce(explosionForce, position, explosionRange);

            if (nearbyObject.GetComponent<Health>())
            {
                int damage = CalculateExplosionDamage(nearbyObject.transform.position, position);
                nearbyObject.GetComponent<Health>().TakeDamage(damage);
            }
        }
    }
    private int CalculateExplosionDamage(Vector3 targetPosition, Vector3 explosionPosition)
    {
        Vector3 explosionToTarget = targetPosition - explosionPosition;

        //find distance
        float explosionDistance = explosionToTarget.magnitude;
        float relativeDistance = (explosionRange - explosionDistance) / explosionRange;

        float damage = relativeDistance * explosionDamage;
        damage = Mathf.Max(explosionDamage / 10, damage);

        return Mathf.RoundToInt(damage); 
    }
}

[CreateAssetMenu(fileName = "New Raycast Bullet", menuName = "Bullets/Raycast Bullet")]
public class RaycastBullet : Bullet
{
    [Header("Effects")]
    public LineRenderer tracer; //effects   
    public ParticleSystem impactEffect;       
    public void Awake()
    {
        shotsLeft = shotsPerBullet; //set value;

    }
    public override void OnFire(Transform weaponOrigin, Transform raycastOrigin)
    {
        //calculate spread        
        float spreadX = Random.Range(-spread, spread);
        float spreadY = Random.Range(-spread, spread);
        Vector3 direction = raycastOrigin.forward + new Vector3(spreadX, spreadY, spreadX);

        //bullet tracers //debug
        ray.origin = weaponOrigin.position;
        ray.direction = direction;
        if (Physics.Raycast(ray, out hit))
            Debug.DrawLine(ray.origin, hit.point, Color.red, 1.0f);

        //if raycast hits
        if (Physics.Raycast(raycastOrigin.position, direction, out hit, range))
        {
            Debug.Log("Target hit, target: " + hit.collider.name);
            Health target = hit.transform.GetComponent<Health>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
            //tracer
            if(tracer)
                SpawnTrail(weaponOrigin, hit.point);

            //explosion
            if (explosionRange > 0)
                Explode(hit.point);
        }
        else if(tracer)
            SpawnTrail(weaponOrigin, ray.GetPoint(range));
        
        //for shotgun
        shotsLeft--;
        if (shotsLeft > 0)
            OnFire(weaponOrigin, raycastOrigin);        
        else
            shotsLeft = shotsPerBullet;                
    }
    
    public void SpawnTrail(Transform origin, Vector3 hitPoint)
    {
        GameObject bulletTrailEffect = Instantiate(tracer.gameObject, origin.position, Quaternion.identity);

        LineRenderer line = bulletTrailEffect.GetComponent<LineRenderer>();

        line.SetPosition(0, origin.position);
        line.SetPosition(1, hitPoint);

        Destroy(bulletTrailEffect, 1f);
    }
}
