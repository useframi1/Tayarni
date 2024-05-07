using System;
using System.Collections.Generic;

namespace API.Entities;

public partial class Airline
{
    public string AirlineName { get; set; }

    public string UniqueCarrier { get; set; }

    public virtual ICollection<Training> Training { get; set; } = new List<Training>();

    public virtual ICollection<UserPrediction> UserPredictions { get; set; } = new List<UserPrediction>();
}
