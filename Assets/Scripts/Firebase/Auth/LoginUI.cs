using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    private GameObject loginPanel;

    //[Header("Profile Button")]
    //[SerializeField]
    //private Button profileButton;

    //[SerializeField]
    //private TextMeshProUGUI profileButtonText;

    [Header("Login Form")]
    [SerializeField]
    private TMP_InputField emailInput;

    [SerializeField]
    private TMP_InputField passwordInput;

    [SerializeField]
    private Button loginButton;

    [SerializeField]
    private Button signupButton;

    [SerializeField]
    private Button anonymousButton;

    [SerializeField]
    private TextMeshProUGUI errorText;

    // [Header("References")]
    // [SerializeField]
    // private ProfileUI profileUI;

    private async UniTaskVoid Start()
    {
        await UniTask.WaitUntil(() => AuthManager.Instance.IsInitialized);
        //await UniTask.WaitUntil(() => profileManager.Instance.IsInitialized);

        // profileButton.onClick.AddListener(OnProfileButtonClicked);
        loginButton.onClick.AddListener(() => OnLoginButtonClicked().Forget());
        signupButton.onClick.AddListener(() => OnSignupButtonClicked().Forget());
        anonymousButton.onClick.AddListener(() => OnAnonymousButtonClicked().Forget());

        UpdateUI().Forget();
    }

    public async UniTaskVoid UpdateUI()
    {
        if (!AuthManager.Instance.IsInitialized) return;

        bool isLogedIn = AuthManager.Instance.IsLogedIn;
        loginPanel.SetActive(!isLogedIn);

        //if (isLogedIn)
        //{
        //    profileButtonText.text = AuthManager.Instance.UserId;
        //}
        //else
        //{
        //    profileButtonText.text = "로그인";
        //}
    }

    private async UniTaskVoid OnLoginButtonClicked()
    {
        string email = emailInput.text.Trim();
        string passwd = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(passwd))
        {
            ShowError("이메일과 비밀번호를 입력하세요");
            return;
        }

        SetButtonsInteractable(false);
        var (success, error) = await AuthManager.Instance.SignInUserWithEmailAsync(email, passwd);
        
        if (success)
        {
            UpdateUI().Forget();
        }
        else
        {
            ShowError(error);
        }

        SetButtonsInteractable(true);
    }

    private async UniTaskVoid OnSignupButtonClicked()
    {
        string email = emailInput.text.Trim();
        string passwd = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(passwd))
        {
            ShowError("이메일과 비밀번호를 입력하세요");
            return;
        }

        SetButtonsInteractable(false);
        var (success, error) = await AuthManager.Instance.CreateUserWithEmailAsync(email, passwd);

        if (success)
        {
            UpdateUI().Forget();
        }
        else
        {
            ShowError(error);
        }

        SetButtonsInteractable(true);
    }

    private async UniTaskVoid OnAnonymousButtonClicked()
    {
        SetButtonsInteractable(false);

        var (success, error) = await AuthManager.Instance.SignInAnonnymouslyAsync();

        if (success)
        {

        }
        else
        {

        }

        SetButtonsInteractable(true);
    }

    //private void OnProfileButtonClicked()
    //{
    //    if (AuthManager.Instance.IsLogedIn)
    //    {
    //        profileUI.OpenProfilePanel().Forget();
    //    }
    //    else
    //    {
    //        loginPanel.SetActive(true);
    //    }
    //}

    private void ShowError(string message)
    {
        errorText.text = message;
        errorText.color = Color.red;
    }

    private void SetButtonsInteractable(bool interactable)
    {
        loginButton.interactable = interactable;
        signupButton.interactable = interactable;
        anonymousButton.interactable = interactable;
    }
}
