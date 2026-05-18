using System;
using System.Collections.Generic;
using System.Text;

namespace SharedKernel.Settings
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiryInDays { get; set; } = 7;
    }
}
