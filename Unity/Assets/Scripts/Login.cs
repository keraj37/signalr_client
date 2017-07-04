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
    public Text status;
    public GameObject hideOnSuccess;

    private void Awake()
    {
        status.text = string.Empty;

        Application.runInBackground = true;
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
        status.text = "<color=#ffa500ff>Authenticating...</color>";
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
                status.text = "<color=#ff0000ff>Error.</color>";
                yield return new WaitForSecondsRealtime(2f);
                status.text = string.Empty;
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

                status.text = "<color=#00ffffff>Authentication successful.</color>";
                hideOnSuccess.SetActive(false);

                yield return new WaitForSecondsRealtime(1.2f);

                SceneManager.LoadScene(1);
            }
        }
    }
}
