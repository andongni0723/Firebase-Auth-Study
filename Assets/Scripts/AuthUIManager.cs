using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AuthUIManager : MonoBehaviour
{
    public static AuthUIManager instance;

    [Header("UI GameObject")]
    public GameObject checkingForAccountUI;
    public GameObject loginUI;
    public GameObject registerUI;
    public GameObject verifyEmailUI;
    public TMP_Text verifyEmail_T;

    private void Awake()
    {
        instance = this;
    }

    private void ClearUI()
    {
        FirebaseManager.instance.ClearOutputs();
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        verifyEmailUI.SetActive(false);
        checkingForAccountUI.SetActive(false);
    }

    public void LoginScreen()
    {
        ClearUI();
        loginUI.SetActive(true);
    }

    public void RegisterScreen()
    {
        ClearUI();
        registerUI.SetActive(true);
    }

    public void AWaitVerifcation(bool _emailSent, string _email, string _output)
    {
        ClearUI();
        verifyEmailUI.SetActive(true);

        if (_emailSent)
        {
            verifyEmail_T.text = $"Sent Email\n Please Verify {_email}";
        }
        else
        {
            verifyEmail_T.text = $"Email Not Sent: {_output}\nPlease Verify {_email}";
        }
    }
}
