using EcommerceApi.Config;
using Microsoft.Extensions.Options;
using Net.payOS;

namespace EcommerceApi.Lib
{
    public class PayOsLibrary
    {
        public readonly PayOS _payOS;
        public readonly PayOsConfig _options;
        public PayOsLibrary(IOptions<PayOsConfig> options)
        {
            _payOS = new PayOS(options.Value.PAYOS_CLIENT_ID, options.Value.PAYOS_API_KEY, options.Value.PAYOS_CHECKSUM_KEY);
            _options = options.Value;
        }
    }
}
