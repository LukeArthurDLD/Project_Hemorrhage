using UnityEngine;

public class LineFade : MonoBehaviour
{
    public Color color;
    public float speed = 10f;

    LineRenderer line;
    void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    void Update()
    {
        color.a = Mathf.Lerp(color.a, 0, Time.deltaTime * speed); //move toward 0

        line.startColor = color;
        line.endColor = color;
    }
}
