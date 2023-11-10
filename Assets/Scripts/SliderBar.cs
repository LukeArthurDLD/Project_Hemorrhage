using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderBar : MonoBehaviour
{
    //technical
    private Slider slider;
    public TMP_Text text;
    public bool percentage = false;

    //colour
    public Gradient gradient;
    public Image fill;

    //camera
    public bool faceCamera = true;
    private Transform cam;

    private void Start()
    {
        // get components
        cam = Camera.main.transform;
        slider = GetComponent<Slider>();
        text = GetComponentInChildren<TMP_Text>();
        if (slider)
            fill = slider.fillRect.GetComponent<Image>();
    }
    public void SetMaxValue(int value)
    {           
        if (slider) // applies value to slider
        {
            slider.maxValue = value;
            slider.value = value;
            if(fill != null)
                fill.color = gradient.Evaluate(1f);            
        }
        if (text) // applies value to text
            text.text = value.ToString();

    }
    public void SetValue(int value)
    {
        if(percentage) // calculates percentage
            value = PercentageValue(value);

        if (slider) // applies value to slider
        {
            slider.value = value;
            if (fill != null)
                fill.color = gradient.Evaluate(slider.normalizedValue);
        }
        if (text) // applies value to text
            text.text = value.ToString();        
    }
    private int PercentageValue(int value)
    {
        return value;        
    }
    private void LateUpdate()
    {
        if (faceCamera)
            transform.LookAt(transform.position + cam.forward);
    }
}
