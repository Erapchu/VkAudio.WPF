namespace VkAudio.WPF.Models
{
    internal class LoginResult
    {
        public string Login { get; }
        public string Password { get; }

        public LoginResult(string login, string password)
        {
            Login = login;
            Password = password;
        }
    }
}
