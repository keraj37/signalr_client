using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using SimpleJSON;

public class Login : MonoBehaviour
{
    private const string LOGIN_URL = @"https://quisutdeus.in/Account/LoginCheck";
    private const string CREDENTIALS_KEY = "storedCredentails";

    public static string LOGGEDIN_NAME = "";

    public InputField email;
    public InputField password;
    public Toggle rememberMe;
    public GameObject loginFailed;
    public GameObject loginSuccess;
    public GameObject hideOnSuccess;

    private void Awake()
    {
        loginFailed.SetActive(false);
        loginSuccess.SetActive(false);
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey(CREDENTIALS_KEY))
        {
            JSONClass json = JSON.Parse(PlayerPrefs.GetString(CREDENTIALS_KEY)).AsObject;
            email.text = json["email"].Value;
            password.text = json["password"].Value;
        }
    }

    public void DoLogin()
    {
        StartCoroutine(LoginCoroutine());
    }

    private IEnumerator LoginCoroutine()
    {
        hideOnSuccess.SetActive(false);

        WWWForm form = new WWWForm();
        form.AddField("email", email.text);
        form.AddField("password", password.text);

        using (UnityWebRequest request = UnityWebRequest.Post(LOGIN_URL, form))
        {
            yield return request.Send();

            bool success = true;

            if (request.isError)
            {
                success = false;                
            }
            else
            {
                JSONClass response = JSON.Parse(request.downloadHandler.text).AsObject;

                if (response["result"].Value != "success")
                    success = false;
            }

            if(!success)
            {
                loginFailed.SetActive(true);
                yield return new WaitForSecondsRealtime(2f);
                loginFailed.SetActive(false);
                hideOnSuccess.SetActive(true);
            }
            else
            {
                if (rememberMe.isOn)
                {
                    JSONClass json = new JSONClass();
                    json.Add("email", email.text);
                    json.Add("password", password.text);

                    PlayerPrefs.SetString(CREDENTIALS_KEY, json.ToString());
                    PlayerPrefs.Save();
                }

                LOGGEDIN_NAME = email.text;

                loginSuccess.SetActive(true);
                hideOnSuccess.SetActive(false);

                yield return new WaitForSecondsRealtime(1.2f);

                SceneManager.LoadScene(1);
            }
        }
    }
}
