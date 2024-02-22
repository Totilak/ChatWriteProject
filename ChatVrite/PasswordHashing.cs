using System.Security.Cryptography;
using System.Text;

public class PasswordHashing
{
    public string HashPassword(string password)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

    public bool VerifyPassword(string inputPassword, string hashedPassword)
    {
        string inputPasswordHash = HashPassword(inputPassword);
        return inputPasswordHash.Equals(hashedPassword);
    }
}
