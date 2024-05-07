using System;
using System.Collections.Generic;

namespace API.Entities;

public partial class User
{
    public string Username { get; set; }

    public string Password { get; set; }

    public virtual ICollection<UserPrediction> UserPredictions { get; set; } = new List<UserPrediction>();
}
