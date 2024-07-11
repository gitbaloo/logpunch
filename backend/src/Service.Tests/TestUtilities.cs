using System;
using System.Collections.Generic;
using System.Linq;
using Domain;
using Microsoft.VisualStudio.TestPlatform.Utilities;

namespace Service.Tests;

public class TestUtilities
{
    public static List<LogpunchUser> AddTestConsultants()
    {
        var x = 1;
        List<LogpunchUser> consultants = new List<LogpunchUser>();

        for (int i = 1; i <= 20; i++)
        {
            LogpunchUser consultant = new LogpunchUser($"testfirstname_{x}", $"testlastname_{x}", $"testuser{x}@techchapter.com", "12345", null);
            consultants.Add(consultant);
            x++;
        }

        return consultants;
    }

    public static List<Domain.LogpunchClient> AddTestCustomers()
    {
        var x = 1;
        List<Domain.LogpunchClient> customers = new List<Domain.LogpunchClient>();

        for (int i = 1; i <= 20; i++)
        {
            Domain.LogpunchClient customer = new Domain.LogpunchClient("TestCompany" + x);
            customers.Add(customer);
            x++;
        }

        return customers;
    }

    public static List<EmployeeClientRelation> AddTestConsultantCustomers(List<Domain.LogpunchClient> customers, List<LogpunchUser> consultants)
    {
        List<EmployeeClientRelation> consultantCustomers = new List<EmployeeClientRelation>();

        var consultantCustomer1 = new EmployeeClientRelation(consultants[0], customers[18]) { Favorite = true };
        var consultantCustomer2 = new EmployeeClientRelation(consultants[0], customers[3]);
        var consultantCustomer3 = new EmployeeClientRelation(consultants[1], customers[0]);
        var consultantCustomer4 = new EmployeeClientRelation(consultants[1], customers[1]);

        consultantCustomers.Add(consultantCustomer1);
        consultantCustomers.Add(consultantCustomer2);
        consultantCustomers.Add(consultantCustomer3);
        consultantCustomers.Add(consultantCustomer4);

        return consultantCustomers;
    }
}
