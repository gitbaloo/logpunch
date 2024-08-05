public static class TestEntityFactory
{
    public static LogpunchUser CreateLogpunchUser(Guid id, string email, string password, string firstName, string lastName, string? defaultQuery, UserRole userRole)
    {
        var user = new LogpunchUser(firstName, lastName, email, password, defaultQuery, userRole);
        SetProtectedProperty(user, nameof(LogpunchUser.Id), id);
        return user;
    }

    public static LogpunchClient CreateLogpunchClient(Guid id, string name)
    {
        var client = new LogpunchClient(name);
        SetProtectedProperty(client, nameof(LogpunchClient.Id), id);
        return client;
    }

    public static EmployeeClientRelation CreateEmployeeClientRelation(Guid id, LogpunchUser employee, LogpunchClient client)
    {
        var employeeClientRelation = new EmployeeClientRelation(employee, client);
        SetProtectedProperty(employeeClientRelation, nameof(EmployeeClientRelation.Id), id);
        return employeeClientRelation;
    }

    public static LogpunchRegistration CreateLogpunchRegistration(Guid id, Guid employeeId, RegistrationType type, int? amount, DateTimeOffset start, DateTimeOffset? end, Guid creatorId, Guid? clientId, DateTimeOffset creationTime, RegistrationStatus status, string? firstComment, string? secondComment, Guid? correctionOfId)
    {
        var registration = new LogpunchRegistration(employeeId, type, amount, start, end, creatorId, clientId, creationTime, status, firstComment, secondComment, correctionOfId);
        SetProtectedProperty(registration, nameof(LogpunchRegistration.Id), id);
        return registration;
    }


    private static void SetProtectedProperty<T>(T obj, string propertyName, object value)
    {
        var property = typeof(T).GetProperty(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy);
        if (property == null)
        {
            throw new ArgumentException($"Property {propertyName} not found on {typeof(T)}");
        }
        property.SetValue(obj, value);
    }
}
