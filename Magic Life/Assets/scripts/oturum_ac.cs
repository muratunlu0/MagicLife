using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
public class oturum_ac : MonoBehaviour {

    protected Firebase.Auth.FirebaseAuth auth;
    protected Firebase.Auth.FirebaseAuth otherAuth;
    protected Dictionary<string, Firebase.Auth.FirebaseUser> userByAuth =
      new Dictionary<string, Firebase.Auth.FirebaseUser>();

    private string logText = "";
    protected string email = "";
    protected string password = "";
    protected string displayName = "";
    protected string phoneNumber = "";
    protected string receivedCode = "";
    private bool fetchingToken = false;
    public bool usePasswordInput = false;
    private Vector2 scrollViewVector = Vector2.zero;

    public Text bildirim_yazisi;
    public int bildirim_suresi = 2;
    public GameObject toast_mesaj_paneli;

    public InputField email_;
    public InputField password_;
    public Toggle beni_hatirla;
    public GameObject yukleniyor_paneli;
    Firebase.Auth.FirebaseUser user;
    private uint phoneAuthTimeoutMs = 60 * 1000;
    // The verification id needed along with the sent code for phone authentication.
    private string phoneAuthVerificationId;
    // Options used to setup secondary authentication object.
    private Firebase.AppOptions otherAuthOptions = new Firebase.AppOptions
    {
        ApiKey = "",
        AppId = "",
        ProjectId = ""
    };

    const int kMaxLogSize = 16382;
    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
    public virtual void Start()
    {
        giris_yap_anonim();
    }
    void debug_kapat()
    {
        toast_mesaj_paneli.SetActive(false);
    }
    public void bildirim_create(string mesaj)
    {
        toast_mesaj_paneli.SetActive(true);
        bildirim_yazisi.text = mesaj;
        Invoke("debug_kapat", bildirim_suresi);
    }

    protected void InitializeFirebase()
    {
        DebugLog("Setting up Firebase Auth");
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        auth.IdTokenChanged += IdTokenChanged;
        
        // Specify valid options to construct a secondary authentication object.
        if (otherAuthOptions != null &&
            !(String.IsNullOrEmpty(otherAuthOptions.ApiKey) ||
              String.IsNullOrEmpty(otherAuthOptions.AppId) ||
              String.IsNullOrEmpty(otherAuthOptions.ProjectId)))
        {
            try
            {
                otherAuth = Firebase.Auth.FirebaseAuth.GetAuth(Firebase.FirebaseApp.Create(
                  otherAuthOptions, "Secondary"));
                otherAuth.StateChanged += AuthStateChanged;
                otherAuth.IdTokenChanged += IdTokenChanged;
            }
            catch (Exception)
            {
                DebugLog("ERROR: Failed to initialize secondary authentication object.");
            }
        }
        GetUserInfo();
        AuthStateChanged(this, null);
    }
    // Output text to the debug log text field, as well as the console.
    public void DebugLog(string s)
    {
        Debug.Log(s);
        logText += s + "\n";

        while (logText.Length > kMaxLogSize)
        {
            int index = logText.IndexOf("\n");
            logText = logText.Substring(index + 1);
        }
        scrollViewVector.y = int.MaxValue;
    }

    // Display user information.
    void DisplayUserInfo(Firebase.Auth.IUserInfo userInfo, int indentLevel)
    {
        string indent = new String(' ', indentLevel * 2);
        var userProperties = new Dictionary<string, string> {
      {"Display Name", userInfo.DisplayName},
      {"Email", userInfo.Email},
      {"Photo URL", userInfo.PhotoUrl != null ? userInfo.PhotoUrl.ToString() : null},
      {"Provider ID", userInfo.ProviderId},
      {"User ID", userInfo.UserId}
    };
        foreach (var property in userProperties)
        {
            if (!String.IsNullOrEmpty(property.Value))
            {
                DebugLog(String.Format("{0}{1}: {2}", indent, property.Key, property.Value));
            }
        }
    }

    // Display a more detailed view of a FirebaseUser.
    void DisplayDetailedUserInfo(Firebase.Auth.FirebaseUser user, int indentLevel)
    {
        DisplayUserInfo(user, indentLevel);
        DebugLog("  Anonymous: " + user.IsAnonymous);
        DebugLog("  Email Verified: " + user.IsEmailVerified);
        var providerDataList = new List<Firebase.Auth.IUserInfo>(user.ProviderData);
        if (providerDataList.Count > 0)
        {
            DebugLog("  Provider Data:");
            foreach (var providerData in user.ProviderData)
            {
                DisplayUserInfo(providerData, indentLevel + 1);
            }
        }
    }

    // Track state changes of the auth object.
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
        Firebase.Auth.FirebaseUser user = null;
        if (senderAuth != null) userByAuth.TryGetValue(senderAuth.App.Name, out user);
        if (senderAuth == auth && senderAuth.CurrentUser != user)
        {
            bool signedIn = user != senderAuth.CurrentUser && senderAuth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                DebugLog("Signed out " + user.UserId);
            }
            user = senderAuth.CurrentUser;
            userByAuth[senderAuth.App.Name] = user;
            if (signedIn)
            {
                DebugLog("Signed in " + user.UserId);
                displayName = user.DisplayName ?? "";
                DisplayDetailedUserInfo(user, 1);
            }
        }
    }
    // Track ID token changes.
    void IdTokenChanged(object sender, System.EventArgs eventArgs)
    {
        Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
        if (senderAuth == auth && senderAuth.CurrentUser != null && !fetchingToken)
        {
            senderAuth.CurrentUser.TokenAsync(false).ContinueWith(
              task => DebugLog(String.Format("Token[0:8] = {0}", task.Result.Substring(0, 8))));
        }
    }

    // Log the result of the specified task, returning true if the task
    // completed successfully, false otherwise.
    bool LogTaskCompletion(Task task, string operation)
    {
        bool complete = false;
        if (task.IsCanceled)
        {
            DebugLog(operation + " canceled.");
        }
        else if (task.IsFaulted)
        {
            DebugLog(operation + " encounted an error.");
            foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
            {
                string authErrorCode = "";
                Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                if (firebaseEx != null)
                {
                    authErrorCode = String.Format("AuthError.{0}: ",
                      ((Firebase.Auth.AuthError)firebaseEx.ErrorCode).ToString());
                }
                DebugLog(authErrorCode + exception.ToString());
            }
        }
        else if (task.IsCompleted)
        {
            DebugLog(operation + " completed");
            complete = true;
        }
        return complete;
    }
    public void giris_yap_anonim()
    {
        GameObject.Find("firebase-message").GetComponent<databasee>().yukleniyor_paneli.SetActive(true);
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.SignInAnonymouslyAsync().ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                bildirim_create("CAnceled");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("IsFaulted");
                bildirim_create("IsFaulted");
                return;
            }
            
            Firebase.Auth.FirebaseUser newUser = task.Result;
            InitializeFirebase();
            GameObject.Find("firebase-message").GetComponent<firebasee>().initcagir_fireabase();
            GameObject.Find("firebase-message").GetComponent<databasee>().intcagir_database();
            // bildirim_create("User signed in successfully");
            Debug.LogFormat("User signed in successfully: {0} ({1})",
        newUser.DisplayName, newUser.UserId);
        });
    }
    void HandleSigninResult(Task<Firebase.Auth.FirebaseUser> authTask)
    {
        //EnableUI();
        LogTaskCompletion(authTask, "Sign-in");
    }

    void GetUserInfo()
    {
        if (auth.CurrentUser == null)
        {
            DebugLog("Not signed in, unable to get info.");
        }
        else
        {
            DebugLog("Current user info:");
            
            DisplayDetailedUserInfo(auth.CurrentUser, 1);
        }
    }
    public void SignOut()
    {
        DebugLog("Signing out.");
        auth.SignOut();
    }
    public Task DeleteUserAsync()
    {
        if (auth.CurrentUser != null)
        {
            DebugLog(String.Format("Attempting to delete user {0}...", auth.CurrentUser.UserId));
            return auth.CurrentUser.DeleteAsync().ContinueWith(HandleDeleteResult);
        }
        else
        {
            DebugLog("Sign-in before deleting user.");
            // Return a finished task.
            return Task.FromResult(0);
        }
    }

    void HandleDeleteResult(Task authTask)
    {
        LogTaskCompletion(authTask, "Delete user");
    }
    public void VerifyPhoneNumber()
    {
        var phoneAuthProvider = Firebase.Auth.PhoneAuthProvider.GetInstance(auth);
        phoneAuthProvider.VerifyPhoneNumber(phoneNumber, phoneAuthTimeoutMs, null,
          verificationCompleted: (cred) => {
              DebugLog("Phone Auth, auto-verification completed");
              auth.SignInWithCredentialAsync(cred).ContinueWith(HandleSigninResult);
          },
          verificationFailed: (error) => {
              DebugLog("Phone Auth, verification failed: " + error);
          },
          codeSent: (id, token) => {
              phoneAuthVerificationId = id;
              DebugLog("Phone Auth, code sent");
          },
          codeAutoRetrievalTimeOut: (id) => {
              DebugLog("Phone Auth, auto-verification timed out");
          });
    }
    public void VerifyReceivedPhoneCode()
    {
        var phoneAuthProvider = Firebase.Auth.PhoneAuthProvider.GetInstance(auth);
        // receivedCode should have been input by the user.
        var cred = phoneAuthProvider.GetCredential(phoneAuthVerificationId, receivedCode);
        auth.SignInWithCredentialAsync(cred).ContinueWith(HandleSigninResult);
    }
    public void hesabısil()
    {
        DeleteUserAsync();
    }
}
