public static class TestEntityFactory
{
    public static LogpunchUser CreateLogpunchUser(Guid id, string email, string password, string firstName, string lastName, string? defaultQuery, UserRole userRole)
    {
        var user = new LogpunchUser
        {
            Id
            Email = email,
            Password = password,
            FirstName = firstName,
            LastName = lastName,
            DefaultQuery = defaultQuery,
            Role = userRole
        };

        SetPrivateProperty(user, nameof(LogpunchUser.Id), id);

        return user;
    }

    public static LogpunchClient CreateLogpunchClient(Guid id, string name)
    {
        var client = new LogpunchClient(name);
        SetPrivateProperty(client, nameof(LogpunchClient.Id), id);
        return client;
    }

    private static void SetPrivateProperty<T>(T obj, string propertyName, object value)
    {
        var property = typeof(T).GetProperty(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (property == null)
        {
            throw new ArgumentException($"Property {propertyName} not found on {typeof(T)}");
        }
        property.SetValue(obj, value);
    }
}
