
using UnityEngine;
using UnityEngine.UI;
public class InteractionMessage : MonoBehaviour
{
    public Text text;
    public string textString;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        textString = text.text;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
