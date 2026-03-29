using System;
using System.Collections.Generic;

namespace API.Models;

public partial class BlacklistedToken
{
    public int Id { get; set; }

    public string Jti { get; set; } = null!;

    public DateTime ExpiryDate { get; set; }
}
