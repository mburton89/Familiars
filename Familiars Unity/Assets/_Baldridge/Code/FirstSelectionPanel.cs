using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FirstSelectionPanel : MonoBehaviour
{
    [SerializeField] Familiar familiar;

    [SerializeField] Color unselectedColor;

    public Familiar Familiar { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        Familiar = familiar;
    }

    public void SetCurrent(bool t)
    {
        if (t)
        {
            gameObject.GetComponent<Image>().color = Color.white;
        }
        else
        {
            gameObject.GetComponent<Image>().color = unselectedColor;
        }
    }
}
