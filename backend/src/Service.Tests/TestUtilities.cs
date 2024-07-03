using System;
using System.Collections.Generic;
using System.Linq;
using Domain;
using Microsoft.VisualStudio.TestPlatform.Utilities;

namespace Service.Tests;

public class TestUtilities
{
    public static List<Consultant> AddTestConsultants()
    {
        var x = 1;
        List<Consultant> consultants = new List<Consultant>();

        for (int i = 1; i <= 20; i++)
        {
            Consultant consultant = new Consultant($"testfirstname_{x}", $"testlastname_{x}", $"testuser{x}@techchapter.com", "12345", null);
            consultants.Add(consultant);
            x++;
        }

        return consultants;
    }

    public static List<Domain.Customer> AddTestCustomers()
    {
        var x = 1;
        List<Domain.Customer> customers = new List<Domain.Customer>();

        for (int i = 1; i <= 20; i++)
        {
            Domain.Customer customer = new Domain.Customer("TestCompany" + x);
            customers.Add(customer);
            x++;
        }

        return customers;
    }

    public static List<ConsultantCustomer> AddTestConsultantCustomers(List<Domain.Customer> customers, List<Consultant> consultants)
    {
        List<ConsultantCustomer> consultantCustomers = new List<ConsultantCustomer>();

        var consultantCustomer1 = new ConsultantCustomer(consultants[0], customers[18]) { Favorite = true };
        var consultantCustomer2 = new ConsultantCustomer(consultants[0], customers[3]);
        var consultantCustomer3 = new ConsultantCustomer(consultants[1], customers[0]);
        var consultantCustomer4 = new ConsultantCustomer(consultants[1], customers[1]);

        consultantCustomers.Add(consultantCustomer1);
        consultantCustomers.Add(consultantCustomer2);
        consultantCustomers.Add(consultantCustomer3);
        consultantCustomers.Add(consultantCustomer4);

        return consultantCustomers;
    }
}
