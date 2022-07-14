using Auth.Models;

namespace Auth.Repositories;

public static class UserRepository
{
    public static User? Get(string userName, string password)
    {
        var users = new List<User>
        {
            new User { Id = 1, UserName = "Jovem", Password = "Peter", Role = "avg" },
            new User { Id = 2, UserName = "Velho", Password = "Carlos", Role = "new" }
        };
        return users.FirstOrDefault(u => string.Equals(u.UserName, userName, StringComparison.CurrentCultureIgnoreCase)
        && u.Password == password);
    }
}