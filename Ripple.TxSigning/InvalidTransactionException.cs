using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ripple.TxSigning
{
    public class InvalidTransactionException : ArgumentException
    {

        public InvalidTransactionException() : base("Transaction is not valid.")
        {
        }

        public InvalidTransactionException(string message) : base(message)
        {
        }

        public InvalidTransactionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidTransactionException(string message, string paramName, Exception innerException) : base(message, paramName, innerException)
        {
        }
    }
}
