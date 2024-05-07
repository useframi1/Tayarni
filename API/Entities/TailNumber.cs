using System;
using System.Collections.Generic;

namespace API.Entities;

public partial class TailNumber
{
    public string TailNum { get; set; }

    public virtual ICollection<Training> Training { get; set; } = new List<Training>();

    public virtual ICollection<UserPrediction> UserPredictions { get; set; } = new List<UserPrediction>();
}
