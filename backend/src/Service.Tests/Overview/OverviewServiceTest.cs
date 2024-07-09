using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using Infrastructure.Overview;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Service.Login;
using Shared;
using Xunit;
using static Service.Tests.TestUtilities;

namespace Service.Tests.Overview;

[TestSubject(typeof(OverviewService))]
public class OverviewServiceTest
{
    private readonly OverviewService _service;
    private readonly LogpunchDbContext _dbContext;
    private readonly ILoginService _loginService;
    private Task<string> token;

    public OverviewServiceTest()
    {
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new LogpunchDbContext(options);

        var mockToken = "mockedBearerToken";

        var mockConsultant = new LogpunchUserDto
        {
            Id = 1,
            Email = "fh@techchapter.com"
        };

        var mockLoginService = new Mock<ILoginService>();

        mockLoginService.Setup(service => service.AuthorizeLogin("fh@techchapter.com", "password1"))
            .ReturnsAsync(mockToken);

        mockLoginService.Setup(service => service.ValidateToken(mockToken))
            .ReturnsAsync(mockConsultant);

        _loginService = mockLoginService.Object;
        token = _loginService.AuthorizeLogin("fh@techchapter.com", "password1");

        _service = new OverviewService(_dbContext, _loginService);

        var consultants = AddTestConsultants();
        var customers = AddTestCustomers();
        _dbContext.AddRange(consultants);
        _dbContext.AddRange(customers);

        _dbContext.SaveChanges();
    }

    /* Sprint 2 Criterion 1
    Given: Period = Past day
    When: Listing
    Then: Show past bank day. Only checking weekends */
    [Fact]
    public async Task SHOW_PAST_BANK_DAY()
    {
        // Arrange
        bool sortAsc = false;
        bool showDaysWithNoRecords = false;
        bool setDefault = false;
        DateTime startDate = new DateTime(2024, 2, 26);
        DateTime? endDate = null;
        string timePeriod = "day";
        string timeMode = "last";
        string groupBy = "client";
        string thenBy = "none";

        string expectedTimeSpan = "Fri. 23/02/2024 - Fri. 23/02/2024";

        // Act
        OverviewResponse result = await _service.OverviewQuery(await token, sortAsc, showDaysWithNoRecords, setDefault,
            startDate, endDate,
            timePeriod, timeMode, groupBy, thenBy);

        // Assert
        Assert.Equal(expectedTimeSpan, result.TimeModePeriodObject.Timespan);
    }

    [Fact]
    public void FIND_FIRST_DAY_OF_THE_WEEK()
    {
        //ARRANGE
        DateTime input = new DateTime(2024, 02, 25);
        DateTime expectedResult = new DateTime(2024, 02, 19);

        //ACT
        var method =
            typeof(OverviewService).GetMethod("FindStartDateOfWeek", BindingFlags.NonPublic | BindingFlags.Instance);
        var result = (DateTime)method.Invoke(_service, [input]);

        //ASSERT
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void FIND_NEXT_VALID_DATE()
    {
        //ARRANGE
        DateTime input = new DateTime(2024, 02, 24);
        DateTime expectedResult = new DateTime(2024, 02, 26);

        //ACT
        var method = typeof(OverviewService).GetMethod("NextValidDate", BindingFlags.NonPublic | BindingFlags.Instance);
        var result = (DateTime)method.Invoke(_service, [input]);

        //ASSERT
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task FIND_FIRST_DAY_OF_THE_MONTH()
    {
        //ARRANGE
        bool sortAsc = false;
        bool showDaysWithNoRecords = false;
        bool setDefault = false;
        DateTime startDate = new DateTime(2024, 2, 26);
        DateTime? endDate = null;
        string timePeriod = "month";
        string timeMode = "current";
        string groupBy = "week";
        string thenBy = "none";

        string expectedDate = "Thu. 01/02/2024 - Thu. 29/02/2024";

        //ACT
        var result = await _service.OverviewQuery(await token, sortAsc, showDaysWithNoRecords, setDefault, startDate,
            endDate,
            timePeriod, timeMode, groupBy, thenBy);


        //ASSERT
        Assert.Equal(expectedDate, result.TimeModePeriodObject.Timespan);
    }

    [Fact]
    public async Task THROW_ERROR_IF_WRONG_TIME_PERIOD()
    {
        // ARRANGE
        bool sortAsc = false;
        bool showDaysWithNoRecords = false;
        bool setDefault = false;
        DateTime startDate = new DateTime(2024, 2, 26);
        DateTime? endDate = null;
        string timePeriod = "thai-buddha-calender";
        string timeMode = "current";
        string groupBy = "week";
        string thenBy = "none";

        // ACT & ASSERT
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _service.OverviewQuery(await token, sortAsc, showDaysWithNoRecords, setDefault, startDate,
                endDate, timePeriod, timeMode, groupBy, thenBy);
        });
    }

    [Fact]
    public async Task THROW_ERROR_IF_WRONG_TIME_MODE()
    {
        // ARRANGE
        bool sortAsc = false;
        bool showDaysWithNoRecords = false;
        bool setDefault = false;
        DateTime startDate = new DateTime(2024, 2, 26);
        DateTime? endDate = null;
        string timePeriod = "month";
        string timeMode = "thai-buddha-calender";
        string groupBy = "week";
        string thenBy = "none";

        // ACT & ASSERT
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _service.OverviewQuery(await token, sortAsc, showDaysWithNoRecords, setDefault, startDate,
                endDate, timePeriod, timeMode, groupBy, thenBy);
        });
    }

    /* Sprint 2 Criterion 2A
    Given: Group by "Customer"
    When: Presenting "Then by" option
    Then: Allow: "Subperiods / Consultants / All registrations" */
    /*[Theory]
    [InlineData("day")]
    [InlineData("week")]
    [InlineData("consultants")]
    [InlineData("allregistrations")]
    public async Task GROUP_BY_CUSTOMERS_THEN_BY(string thenBy)
    {
        // Arrange
        bool sortAsc = false;
        bool showDaysWithNoRecords = false;
        bool setDefault = false;
        DateTime startDate = DateTime.Today.Date;
        DateTime? endDate = null;
        string timePeriod = "month";
        string timeMode = "current";
        string groupBy = "client";

        // Act
        var exception = await Record.ExceptionAsync(() => _service.OverviewQuery(sortAsc, showDaysWithNoRecords, setDefault, startDate, endDate, timePeriod, timeMode, groupBy, thenBy));

        // Assert
        Assert.Null(exception);
    }*/


    /* Sprint 2 Criterion 2B
    Given: Group by Period
    When: Presenting "Then by" option
    Then: Allow: "Subperiod / Customers / All Registrations */
    /*[Theory]
    [InlineData("day")]
    [InlineData("week")]
    [InlineData("clients")]
    [InlineData("allregistrations")]
    public async Task GROUP_BY_PERIOD(string thenBy)
    {
        // Arrange
        bool sortAsc = false;
        bool showDaysWithNoRecords = false;
        bool setDefault = false;
        DateTime startDate = DateTime.Today.Date;
        DateTime? endDate = null;
        string timePeriod = "month";
        string timeMode = "current";
        string groupBy = "period";

        // Act
        var exception = await Record.ExceptionAsync(() => _service.OverviewQuery(sortAsc, showDaysWithNoRecords, setDefault, startDate, endDate, timePeriod, timeMode, groupBy, thenBy));

        // Assert
        Assert.Null(exception);
    }*/

    /* Sprint 2 Criterion 3
    Given: "Group by" is selected
    AND no "Then by"
    When: Showing overview
    Then: Omit "Then by's" in overview */
    /*[Theory]
    [InlineData("client")]
    [InlineData("periods")]
    public async Task GROUP_BY_IS_SELECTED(string groupBy)
    {
        // Arrange
        string token =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6ImZoQHRlY2hjaGFwdGVyLmNvbSIsIm5hbWVpZCI6IjEiLCJleHAiOjE3MDkwMjIyNTksImlzcyI6Imh0dHBzOi8vcHVuY2hsb2cuaW8iLCJhdWQiOiJQdW5jaGxvZ0FQSSJ9.dLtJQTZaRvUpog-cYa0dNlptgMDGiy19ivh9fUZMrN8";
        bool sortAsc = false;
        bool showDaysWithNoRecords = false;
        bool setDefault = false;
        DateTime startDate = DateTime.Today.Date;
        DateTime? endDate = null;
        string timePeriod = "week";
        string timeMode = "current";
        string thenBy = "none";

        // Act
        var exception = await Record.ExceptionAsync(() => _service.OverviewQuery(token, sortAsc, showDaysWithNoRecords, setDefault, startDate, endDate, timePeriod, timeMode, groupBy, thenBy));


        // Assert
        Assert.Null(exception);
    }*/

    // TODO! Missing default property for consultant in 

    /* Sprint 2 Criterion 4
    Given: Query selected
    When: "Set default" is set
    Then: Save as default query for consultant*/
    /*[Fact]
    public void SET_DEFAULT()
    {
        // Arrange
        bool sortAsc = false;
        bool showDaysWithNoRecords = false;
        bool setDefault = true;
        DateTime startDate = DateTime.Today.Date;
        DateTime? endDate = null;
        string timePeriod = "month";
        string timeMode = "rolling";
        string groupBy = "period";
        string thenBy = "day";
        var overviewSettingNewDefault = _service.OverviewQuery(sortAsc, showDaysWithNoRecords, setDefault, startDate, endDate, timePeriod, timeMode, groupBy, thenBy);

        // Act

        // Assert

    }*/

    /* Sprint 2 Criterion 5A
    Given: "Group by" = month
    When: Presenting "Then by" Periods
    Then: Allow: "Customer / All Registrations / Sub period = Day / Week"
    */
    /*[Theory]
    [InlineData("week")]
    [InlineData("day")]
    public void GROUP_BY_MONTH(string thenBy)
    {
        // Arrange
        bool sortAsc = false;
        bool showDaysWithNoRecords = false;
        bool setDefault = true;
        DateTime startDate = DateTime.Today.Date;
        DateTime? endDate = null;
        string timePeriod = "month";
        string timeMode = "rolling";
        string groupBy = "period";

        // Act


        // Assert


    }*/


    /* Sprint 2 Criterion 5B
    Given: "Group by" = Week
    When: Presenting "Then by" Periods
    Then: Allow: "Customer / All Registrations / Sub period = Day"
    */
    /*[Fact]
    public async Task GROUP_BY_WEEK()
    {
        // Arrange
        bool sortAsc = false;
        bool showDaysWithNoRecords = false;
        bool setDefault = true;
        DateTime startDate = DateTime.Today.Date;
        DateTime? endDate = null;
        string timePeriod = "week";
        string timeMode = "rolling";
        string groupBy = "periods";
        string thenBy = "day";

        // Act


        // Assert



    }*/


    /*/// Sprint 2 Criterion 5C
    /// Given: "Group by" = Day
    /// When: Presenting "Then by" Periods
    /// Then: Allow: "Customer / All Registrations"*/
    /*[Fact]
    public async Task GROUP_BY_DAY()
    {
        // Arrange
        bool sortAsc = false;
        bool showDaysWithNoRecords = false;
        bool setDefault = true;
        DateTime startDate = DateTime.Today.Date;
        DateTime? endDate = null;
        string timePeriod = "day";
        string timeMode = "rolling";
        string groupBy = "periods";
        string subPeriod = "none";
        bool consultants = false;
        bool customers = false;
        bool allRegistrations = false;

        // Act


        // Assert


    }*/

    /// Sprint 2 Criterion 5D
    /// Given: "Group by" = Year
    /// When: Presenting "Then by" Periods
    /// Then: Allow: "Customer / All Registrations / Sub period = Month / Week"
    /*[Theory]
    [InlineData("week")]
    [InlineData("month")]
    public async Task GROUP_BY_YEAR(string subPeriod)
    {
        // Arrange
        bool sortAsc = false;
        bool showDaysWithNoRecords = false;
        bool setDefault = true;
        DateTime startDate = DateTime.Today.Date;
        DateTime? endDate = null;
        string timePeriod = "year";
        string timeMode = "rolling";
        string groupBy = "periods";
        bool consultants = false;
        bool customers = false;
        bool allRegistrations = false;

        // Act


        // Assert



    }*/

    /* Sprint 2 Criterion 6A
    Given: Option "Sort Ascending" is set
    When: Showing overview
    Then: Sort "Group by" AND "Then by" Ascending
    */
    /*[Fact]
    public async Task SET_ASC()
    {
        // Arrange
        bool sortAsc = false;
        bool showDaysWithNoRecords = false;
        bool setDefault = false;
        DateTime startDate = DateTime.Today.Date;
        DateTime? endDate = null;
        string timePeriod = "week";
        string timeMode = "last";
        string groupBy = "periods";
        string thenBy = "customers";

        // Act

        // Assert

    }*/


    /* Sprint 2 Criterion 6B
    Given: Option "Sort Descending" is unset
    When: Showing overview
    Then: Sort "Group by" AND "Then by" Descending
    */

    /* Sprint 2 Criterion 7
    Given: No default consultant query
    When: Entering overview
    Then: Show consultants rolling week
    "Grouped by" Days / "Then by" Customer"
    */

    /* Sprint 2 Criterion 8A
    Given: Option: "Show days without records" is set
    When: Showing overview
    Then: Don't include days without records
    */

    /* Sprint 2 Criterion 8B
    Given: Option: "Show days without records" is unset
    When: Showing overview
    Then: Show 0 hrs. for days without records
    */

    /* Sprint 2 Criterion 9
    Given: Relative period (current/ rolling/last)
    When: Copying deep link
    Then: Include relativeness in link
    */
}
