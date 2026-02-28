using System;

namespace API.Utils.JwtProvider;

public class JwtOptions
{
    public string SecretAccess { get; set; } = null!;
    public string SecretRefresh { get; set; } = null!;
}
