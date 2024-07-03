using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using Npgsql;
using NpgsqlTypes;

namespace Domain;

public class TimeRegistration : Entity
{
    public double Hours { get; set; }

    [Column("registration_date")]
    public DateTime RegistrationDate { get; set; }
    public int ConsultantCustomerId { get; set; }

    public TimeRegistration(double hours, int consultantCustomerId)
    {
        Hours = hours;
        var utcNow = DateTime.UtcNow;
        RegistrationDate = utcNow;
        ConsultantCustomerId = consultantCustomerId;
    }
}
