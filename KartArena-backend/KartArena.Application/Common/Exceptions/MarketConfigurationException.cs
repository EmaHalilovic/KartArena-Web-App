using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Application.Common.Exceptions
{
    public sealed class MarketConfigurationException : Exception
    {
        public string Code { get; }

        public MarketConfigurationException(
            string code,
            string message)
            : base(message)
        {
            Code = code;
        }
    }
}
