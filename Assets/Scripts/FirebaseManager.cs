using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;

    [Header("Firebase")]
    public FirebaseAuth auth;
    public FirebaseUser user;
    [Space(5f)]

    [Header("Login UI")]
    public TMP_InputField loginEmail_IF;
    public TMP_InputField loginPassword_IF;
    public TMP_Text loginOutput_T;
    [Space(5f)]

    [Header("Register UI")]
    public TMP_InputField registerUserName_IF;
    public TMP_InputField registerEmail_IF;
    public TMP_InputField registerPassword_IF;
    public TMP_InputField registerConfirmPassword_IF;
    public TMP_Text registerOutput_T;
    

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        instance = this;

        
    }

    private void Start()
    {
        StartCoroutine(CheckAndFixDependencing());
    }

    private IEnumerator CheckAndFixDependencing()
    {
        var checkAndFixDependencingTask = FirebaseApp.CheckAndFixDependenciesAsync();

        yield return new WaitUntil(predicate: () => checkAndFixDependencingTask.IsCompleted);

        var dependencyResult = checkAndFixDependencingTask.Result;

        if (dependencyResult == DependencyStatus.Available)
        {
            InitiallzeFirebase();
        }
        else
        {
            Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyResult}");
        }
    }

    private void InitiallzeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        StartCoroutine(CheckAutoLogin());

        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    private IEnumerator CheckAutoLogin()
    {
        yield return new WaitForEndOfFrame();

        if (user != null)
        {
            var reloadUserTask = user.ReloadAsync();

            yield return new WaitUntil(predicate: () => reloadUserTask.IsCompleted);

            AutoLogin();
        }
        else
        {
            AuthUIManager.instance.LoginScreen();
        }
    }

    public void AutoLogin()
    {
        if (user != null)
        {
            if (user.IsEmailVerified)
            {
                GameManager.instance.ChangeScene(1);
            }
            else
            {
                StartCoroutine(SentVerificationEmail());
            }
        }
        else
        {
            AuthUIManager.instance.LoginScreen();
        }
    }   

    private void AuthStateChanged(object sender,System.EventArgs eventArgs)
    {
        if(auth.CurrentUser != user)
        {
            bool signIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signIn && user != null)
            {
                Debug.Log("Signed Out");
            }

            user = auth.CurrentUser;

            if (signIn)
            {
                Debug.Log($"Signed In: {user.DisplayName}");
            }
        }
    }

    public void ClearOutputs()
    {
        loginOutput_T.text = "";
        registerOutput_T.text = "";
    }

    public void LoginButton()
    {
        StartCoroutine(LoginLogic(loginEmail_IF.text, loginPassword_IF.text));
    }

    public void RegisterButton()
    {
        StartCoroutine(RegisterLogic(registerUserName_IF.text, registerEmail_IF.text, registerPassword_IF.text, registerConfirmPassword_IF.text));
    }

    private IEnumerator LoginLogic(string _email,string _password)
    {
        Credential credential = EmailAuthProvider.GetCredential(_email, _password);

        var loginTask = auth.SignInAndRetrieveDataWithCredentialAsync(credential);

        yield return new WaitUntil(predicate: () => loginTask.IsCompleted);

        if(loginTask.Exception != null)
        {
            FirebaseException firebaseException = (FirebaseException)loginTask.Exception.GetBaseException();
            AuthError error = (AuthError)firebaseException.ErrorCode;
            string output = "Unknown Error, Please try Again";

            switch (error)
            {
                case AuthError.MissingEmail:
                    output = "Please Enter Your Email";
                    break;
                case AuthError.MissingPassword:
                    output = "Please Enter Your Password";
                    break;
                case AuthError.InvalidEmail:
                    output = "Invalid Email";
                    break;
                case AuthError.WrongPassword:
                    output = "Wrong Password";
                    break;
                case AuthError.UserNotFound:
                    output = "User Not Found";
                    break;
            }

            loginOutput_T.text = output;
        }
        else
        {
            if (user.IsEmailVerified)
            {
                yield return new WaitForSeconds(1);
                GameManager.instance.ChangeScene(1);
            }
            else
            {
                StartCoroutine(SentVerificationEmail());
            }
        }
    }

    public IEnumerator RegisterLogic(string _userName, string _email, string _password, string _confirmPassword)
    {
        if (_userName == "")
        {
            registerOutput_T.text = "Please Enter A Username";
        }
        else if(_password != _confirmPassword)
        {
            registerOutput_T.text = "Passwords Do Not Match";
        }
        else
        {
            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);

            yield return new WaitUntil(predicate: () => registerTask.IsCompleted);

            if (registerTask.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException)registerTask.Exception.GetBaseException();
                AuthError error = (AuthError)firebaseException.ErrorCode;
                string output = "Unknown Error, Please try Again";

                switch (error)
                {
                    case AuthError.InvalidEmail:
                        output = "Invalid Email";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        output = "Email Already In Use";
                        break;
                    case AuthError.WeakPassword:
                        output = "Invalid Email";
                        break;
                    case AuthError.MissingEmail:
                        output = "Please Enter Your Email";
                        break;
                    case AuthError.MissingPassword:
                        output = "Please Enter Your Password";
                        break;
                }

                registerOutput_T.text = output;
            }
            else
            {
                UserProfile profile = new UserProfile
                {
                    DisplayName = _userName,

                    //Give Profite Default Photos
                    PhotoUrl = new System.Uri("https://pbs.twimg.com/media/EFKdt0bWsAIfcj9.jpg"),
                };

                var defaultItUserTask = user.UpdateUserProfileAsync(profile);

                yield return new WaitUntil(predicate: () => defaultItUserTask.IsCompleted);

                if (registerTask.Exception != null)
                {
                    user.DeleteAsync();

                    FirebaseException firebaseException = (FirebaseException)defaultItUserTask.Exception.GetBaseException();
                    AuthError error = (AuthError)firebaseException.ErrorCode;
                    string output = "Unknown Error, Please try Again";

                    switch (error)
                    {
                        case AuthError.Cancelled:
                            output = "Update User Cancelled";
                            break;
                        case AuthError.SessionExpired:
                            output = "Session Expired";
                            break;                       
                    }

                    registerOutput_T.text = output;
                }
                else
                {
                    Debug.Log($"Firebase User Created Successfully: {user.DisplayName} ({user.UserId})");

                    StartCoroutine(SentVerificationEmail());
                }
            }
        }
    }

    private IEnumerator SentVerificationEmail()
    {
        if(user != null)
        {
            var emailTask = user.SendEmailVerificationAsync();

            yield return new WaitUntil(predicate: () => emailTask.IsCompleted);

            if (emailTask.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException)emailTask.Exception.GetBaseException();
                AuthError error = (AuthError)firebaseException.ErrorCode;

                string output = "Unknow Error, Try Again";

                switch (error)
                {
                    case AuthError.Cancelled:
                        output = "Verifcation Task was Cancelled";
                        break;
                    case AuthError.InvalidRecipientEmail:
                        output = "Invalid Email";
                        break;
                    case AuthError.TooManyRequests:
                        output = "Too Many Requests";
                        break;
                }

                AuthUIManager.instance.AWaitVerifcation(false, user.Email, output);
            }
            else
            {
                AuthUIManager.instance.AWaitVerifcation(true, user.Email, null);
                Debug.Log("Email Sent Successfully");
            }
        }
    }

    public void UpdateProfilePicture(string _newPfpURL)
    {
        StartCoroutine(UpdateProfilePictureLogic(_newPfpURL));
    }

    private IEnumerator UpdateProfilePictureLogic(string _newPfpURL)
    {
        if(user != null)
        {
            UserProfile profile = new UserProfile();

            try
            {
                UserProfile _profile = new UserProfile
                {
                    PhotoUrl = new System.Uri(_newPfpURL),
                };

                profile = _profile;
            }
            catch
            {
                LobbyManager.instance.Output("Error Fetching Image, Make Sure Your Link is Vaild");
                yield break;
            }

            var pfpTask = user.UpdateUserProfileAsync(profile);

            yield return new WaitUntil(predicate: () => pfpTask.IsCompleted);

            if (pfpTask.Exception != null)
            {
                Debug.LogError($"Updating Profile Picture was unsuccessful: {pfpTask.Exception}");
            }
            else
            {    
                LobbyManager.instance.ChangePfpSuccess();
                Debug.Log("Profile Image Updated Successfully");
            }
        }
    }
}
