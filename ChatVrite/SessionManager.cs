using System;
using System.IO;

public static class SessionManager
{
    private const string UserDataFilePath = "UserData.txt";
    private const int SessionDurationInMinutes = 100;

    public static string Username
    {
        get
        {
            if (File.Exists(UserDataFilePath))
            {
                string[] userData = File.ReadAllLines(UserDataFilePath);
                if (userData.Length == 3 && IsSessionValid(userData[2])) // Проверка корректного количества строк
                {
                    return userData[0];
                }
                else
                {
                    ClearUserData();
                }
            }
            return null;
        }
        set
        {
            string currentTime = DateTime.Now.ToString("o");
            File.WriteAllText(UserDataFilePath, $"{value}\n{GetSessionPassword()}\n{currentTime}");
        }
    }

    public static string GetSessionPassword()
    {
        if (File.Exists(UserDataFilePath))
        {
            string[] userData = File.ReadAllLines(UserDataFilePath);
            if (userData.Length == 3 && IsSessionValid(userData[2]))
            {
                return userData[1];
            }
            else
            {
                ClearUserData();
            }
        }
        return null;
    }

    public static void SetUserCredentials(string username, string password)
    {
        string currentTime = DateTime.Now.ToString("o");
        File.WriteAllText(UserDataFilePath, $"{username}\n{password}\n{currentTime}");
    }

    public static bool IsUserAuthenticated()
    {
        return !string.IsNullOrEmpty(Username);
    }

    public static void ClearUserData()
    {
        if (File.Exists(UserDataFilePath))
        {
            File.Delete(UserDataFilePath);
        }
    }
    public static string GetSavedPassword()
    {
        if (File.Exists(UserDataFilePath))
        {
            string[] userData = File.ReadAllLines(UserDataFilePath);
            if (userData.Length == 3 && IsSessionValid(userData[2]))
            {
                return userData[1];
            }
            else
            {
                ClearUserData();
            }
        }
        return null;
    }

    private static bool IsSessionValid(string sessionTime)
    {
        DateTime createdTime;
        if (DateTime.TryParse(sessionTime, out createdTime))
        {
            TimeSpan elapsedTime = DateTime.Now - createdTime;
            return elapsedTime.TotalMinutes <= SessionDurationInMinutes;
        }
        return false;
    }
}
