using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;        

public class ButtonHighlight : MonoBehaviour {

    Button button;
    static EventSystem eventSystem;

	void Awake () {
        button = GetComponent<Button>();
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
    }

    public void Highlight()
    {
        button.Select();
        Invoke("Deselect", 0.1f);
    }

    void Deselect()
    {
        eventSystem.SetSelectedGameObject(null);
    }

    public void HighlightToggle(bool caps)
    {
        if (caps) button.Select();
        else eventSystem.SetSelectedGameObject(null);
    }
}
