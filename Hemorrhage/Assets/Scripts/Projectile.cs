using UnityEngine;

public class Projectile : MonoBehaviour
{
    [System.NonSerialized]
    private Rigidbody rb;
    public GameObject explosionEffect;

    [Range(0f, 1f)]
    public float bounciness;
    public bool useGravity;

    //damage
    public int damage;

    //explosion
    public int explosionDamage;
    public float explosionRange;
    public float explosionForce;

    //lifetime
    public int minCollisions;
    public int maxCollisions;
    public bool explodeOnTouch;
    private int collisions;
    public float maxLifetime;

    PhysicMaterial physicsMat;
    void Start()
    {
        Setup();
    }
    void FixedUpdate()
    {
        //manage lifetime
        if(collisions >= minCollisions)
            maxLifetime -= Time.deltaTime;
        if (maxLifetime <= 0)
            Explode();

    }
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Health>())
            collision.gameObject.GetComponent<Health>().TakeDamage(damage);

        collisions++;
        if (collisions >= maxCollisions)
            Explode();

        if (collision.gameObject.GetComponent<Health>() && explodeOnTouch)
            Explode();
    }
    void Setup()
    {
        rb = GetComponent<Rigidbody>();

        //set physics
        physicsMat = new PhysicMaterial();
        physicsMat.bounciness = bounciness;
        physicsMat.frictionCombine = PhysicMaterialCombine.Minimum;
        physicsMat.bounceCombine = PhysicMaterialCombine.Maximum;
        GetComponent<SphereCollider>().material = physicsMat;

        rb.useGravity = useGravity;
    }
    public void Explode()
    {
        if (explosionEffect)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(explosion, 1);
        }

        Collider[] targets = Physics.OverlapSphere(transform.position, explosionRange);
        foreach (Collider nearbyObject in targets)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddExplosionForce(explosionForce, transform.position, explosionRange);

            if (nearbyObject.GetComponent<Health>())
            {
                int damage = CalculateExplosionDamage(nearbyObject.transform.position, transform.position);
                nearbyObject.GetComponent<Health>().TakeDamage(damage);
            }
        }
        Destroy(gameObject);
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
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }

}
