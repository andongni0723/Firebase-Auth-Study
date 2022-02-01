using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;

    [Header("UI Panel")]
    public GameObject profileUI;
    public GameObject changeProfileUI;
    public GameObject changeEmailUI;
    public GameObject changePasswordUI;
    public GameObject reverifyUI;
    public GameObject resetPasswordConfirmUI;
    public GameObject actionSuccessPanelUI;
    public GameObject deletUserConifmUI;
    [Space(5)]

    [Header("Basic Info UI")]
    public TMP_Text userName_T;
    public TMP_Text email_T;
    public string token;
    [Space(5)]

    [Header("Profile Picture UI")]
    public Image profilePicture;
    public TMP_InputField profilePictureLink_IF;
    public TMP_Text output_T;
    [Space(5)]

    [Header("Change Email UI")]
    public TMP_InputField changeEmail_IF;
    [Space(5)]

    [Header("Change Password UI")]
    public TMP_InputField changePassword_IF;
    public TMP_InputField changePasswordConfirm_IF;
    [Space(5)]

    [Header("Reverify UI")]
    public TMP_InputField reverifyEmail_IF;
    public TMP_InputField reverifyPassword_IF;
    [Space(5)]

    [Header("Action Success Panel UI")]
    public TMP_Text actionSuccess_T;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        print(FirebaseManager.instance.user);
        if(FirebaseManager.instance.user != null)
        {
            LoadProfile();
        }
        
    }

    private void LoadProfile()
    {
        if (FirebaseManager.instance.user != null)
        {
            // Set Varibles
            System.Uri photoUrl = FirebaseManager.instance.user.PhotoUrl;
            string name = FirebaseManager.instance.user.DisplayName;
            string email = FirebaseManager.instance.user.Email;


            // Set UI
            StartCoroutine(LoadImage(photoUrl.ToString()));
            userName_T.text = name;
            email_T.text = email;
        }
    }

    private IEnumerator LoadImage(string _photoUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(_photoUrl);

        yield return request.SendWebRequest();

        if (request.error != null)
        {
            string output = "Unknow Error! TryAgain!";

            if (request.result == UnityWebRequest.Result.ProtocolError)
            {
                output = "Image Type Not Supported! Please Try Another Image";

            }

            Output(output);
        }
        else
        {
            Texture2D image = ((DownloadHandlerTexture)request.downloadHandler).texture;

            profilePicture.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.zero);
        }
    }

    public void Output(string _output)
    {
        output_T.text = _output;
    }

    public void ClearUI()
    {
        output_T.text = "";
        profileUI.SetActive(false);
        changeProfileUI.SetActive(false);
        // Change Email
        // Change Password
        // Reverify
        // Reset Password
        actionSuccessPanelUI.SetActive(false);
        // Delet User
    }

    public void ProfileUI()
    {
        ClearUI();
        profileUI.SetActive(true);
        LoadProfile();
    }

    public void ChangePfpUI()
    {
        ClearUI();
        changeProfileUI.SetActive(true);
    }

    public void ChangePfpSuccess()
    {
        ClearUI();
        actionSuccessPanelUI.SetActive(true);
        actionSuccess_T.text = "Profile Picture Changed Successfully";
    }

    public void SubmitProfileImageButton()
    {
        FirebaseManager.instance.UpdateProfilePicture(profilePictureLink_IF.text);
    }
}
