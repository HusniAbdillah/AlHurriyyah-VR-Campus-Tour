using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class HoverAnimationController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale Animation")]
    public bool useScaleAnimation = true;
    public float hoverScaleMultiplier = 1.1f;
    public float scaleDuration = 0.2f;
    
    [Header("Color Animation")]
    public bool useColorAnimation = true;
    public Color hoverColor = new Color(1f, 1f, 1f, 1f);
    public float colorDuration = 0.2f;
    
    [Header("Rotation Animation")]
    public bool useRotationAnimation = false;
    public float rotationAmount = 5f;
    public float rotationDuration = 0.2f;
    
    private Vector3 originalScale;
    private Color originalColor;
    private Quaternion originalRotation;
    private Image image;
    private Coroutine scaleCoroutine;
    private Coroutine colorCoroutine;
    private Coroutine rotationCoroutine;
    
    void Awake()
    {
        originalScale = transform.localScale;
        image = GetComponent<Image>();
        if (image) originalColor = image.color;
        originalRotation = transform.localRotation;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (useScaleAnimation)
        {
            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
            scaleCoroutine = StartCoroutine(AnimateScale(originalScale * hoverScaleMultiplier));
        }
        
        if (useColorAnimation && image)
        {
            if (colorCoroutine != null) StopCoroutine(colorCoroutine);
            colorCoroutine = StartCoroutine(AnimateColor(hoverColor));
        }
        
        if (useRotationAnimation)
        {
            if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);
            rotationCoroutine = StartCoroutine(AnimateRotation(Quaternion.Euler(0, 0, rotationAmount)));
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (useScaleAnimation)
        {
            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
            scaleCoroutine = StartCoroutine(AnimateScale(originalScale));
        }
        
        if (useColorAnimation && image)
        {
            if (colorCoroutine != null) StopCoroutine(colorCoroutine);
            colorCoroutine = StartCoroutine(AnimateColor(originalColor));
        }
        
        if (useRotationAnimation)
        {
            if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);
            rotationCoroutine = StartCoroutine(AnimateRotation(originalRotation));
        }
    }
    
    IEnumerator AnimateScale(Vector3 targetScale)
    {
        float time = 0;
        Vector3 startScale = transform.localScale;
        
        while (time < scaleDuration)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, time / scaleDuration);
            time += Time.deltaTime;
            yield return null;
        }
        
        transform.localScale = targetScale;
    }
    
    IEnumerator AnimateColor(Color targetColor)
    {
        float time = 0;
        Color startColor = image.color;
        
        while (time < colorDuration)
        {
            image.color = Color.Lerp(startColor, targetColor, time / colorDuration);
            time += Time.deltaTime;
            yield return null;
        }
        
        image.color = targetColor;
    }
    
    IEnumerator AnimateRotation(Quaternion targetRotation)
    {
        float time = 0;
        Quaternion startRotation = transform.localRotation;
        
        while (time < rotationDuration)
        {
            transform.localRotation = Quaternion.Lerp(startRotation, targetRotation, time / rotationDuration);
            time += Time.deltaTime;
            yield return null;
        }
        
        transform.localRotation = targetRotation;
    }
}