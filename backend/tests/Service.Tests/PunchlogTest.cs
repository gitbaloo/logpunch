using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace Service.Tests;

public class PunchlogTest
{
    private readonly TimeRegistrationService _timeRegistrationService;

    public PunchlogTest()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<PunchlogTest>()
            .Build();

        _timeRegistrationService = new TimeRegistrationService(configuration);
    }
    
    
}