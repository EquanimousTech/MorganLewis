using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class TagData
{
    public string title;
    public string content;
    public string author;
    public Color color = Color.white;
}

public class TagDisplay : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI contentText;
    public TextMeshProUGUI authorText;
    public Image backgroundImage;
    
    public void SetData(TagData data)
    {
        if (titleText) titleText.text = data.title;
        if (contentText) contentText.text = data.content;
        if (authorText) authorText.text = data.author;
        if (backgroundImage) backgroundImage.color = data.color;
    }
}

public class HangingAnimation : MonoBehaviour
{
    public float swingAmount = 10f;
    public float swingSpeed = 1f;
    private float randomOffset;
    
    void Start()
    {
        randomOffset = Random.Range(0f, 2f * Mathf.PI);
    }
    
    void Update()
    {
        float swing = Mathf.Sin(Time.time * swingSpeed + randomOffset) * swingAmount;
        transform.rotation = Quaternion.Euler(0, 0, swing);
    }
}