using System;

namespace Rohmote
{
    public class RpcException : Exception
    {
        internal RpcException(string error)
            : base(error)
        {

        }

        public override string StackTrace
        {
            get { return null; }
        }

        public override string ToString()
        {
            return typeof(RpcException).FullName + ": " + Message;
        }
    }
}
