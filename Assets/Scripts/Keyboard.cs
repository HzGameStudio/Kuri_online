using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Keabord : MonoBehaviour
{
    TouchScreenKeyboard userKeyboard;
    //Text text;
    string keyboardText;
    //InputField 

    public void OpenKeyBoard()
    {
        userKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
        userKeyboard.active = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(TouchScreenKeyboard.visible == false && userKeyboard != null)
        {
            if(userKeyboard.done)
            {
                //keyboardText = userKeyboard.text;
                userKeyboard = null;
            }
            
        }
    }
}
