using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Application.Common.Exceptions
{
    public sealed class MarketExternalServiceException : Exception
    {
        public string Code { get; }

        public MarketExternalServiceException(
            string code,
            string message,
            Exception? innerException = null)
            : base(message, innerException)
        {
            Code = code;
        }
    }
}
