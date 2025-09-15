using SampleTestProject.Models;

namespace SampleTestProject.Services;

public interface IUserService
{
    User CreateUser(UserData userData);
    User GetUser(int id);
    void UpdateUser(int id, UserData userData);
    void DeleteUser(int id);
}

public class UserService : IUserService
{
    private readonly List<User> _users = new();
    private int _nextId = 1;

    public User CreateUser(UserData userData)
    {
        if (string.IsNullOrEmpty(userData.Name))
            throw new ValidationException("Name is required");

        if (string.IsNullOrEmpty(userData.Email))
            throw new ValidationException("Email is required");

        if (!userData.Email.Contains("@"))
            throw new ValidationException("Invalid email format");

        var user = new User
        {
            Id = _nextId++,
            Name = userData.Name,
            Email = userData.Email,
            CreatedAt = DateTime.Now
        };

        _users.Add(user);
        return user;
    }

    public User GetUser(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            throw new ArgumentException($"User with id {id} not found");

        return user;
    }

    public void UpdateUser(int id, UserData userData)
    {
        var user = GetUser(id);

        if (!string.IsNullOrEmpty(userData.Name))
            user.Name = userData.Name;

        if (!string.IsNullOrEmpty(userData.Email))
        {
            if (!userData.Email.Contains("@"))
                throw new ValidationException("Invalid email format");
            user.Email = userData.Email;
        }
    }

    public void DeleteUser(int id)
    {
        var user = GetUser(id);
        _users.Remove(user);
    }
}
