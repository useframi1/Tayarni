using System;
using System.Collections.Generic;

namespace API.Entities;

public partial class Airport
{
    public string AirportName { get; set; }

    public string AirportCode { get; set; }

    public virtual ICollection<Training> TrainingDestAirportNavigations { get; set; } = new List<Training>();

    public virtual ICollection<Training> TrainingOrgAirportNavigations { get; set; } = new List<Training>();

    public virtual ICollection<UserPrediction> UserPredictionDestAirportNavigations { get; set; } = new List<UserPrediction>();

    public virtual ICollection<UserPrediction> UserPredictionOrgAirportNavigations { get; set; } = new List<UserPrediction>();
}
