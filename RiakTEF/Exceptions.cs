using System;
using RiakClient;
using RiakClient.Commands.TS;

namespace RiakTEF
{
    public class GeneralException : Exception
    {
        public GeneralException(string message, ResultCode resultCode)
            : base(message)
        {
            ErrorCode = resultCode;
        }

        public ResultCode ErrorCode { get; }
    }

    public class QueryException : GeneralException
    {
        public QueryException(RiakResult result, Query query)
            : base(result.ErrorMessage, result.ResultCode)
        {
            Query = query.Options.Query.ToString();
        }

        public string Query { get; }
    }

    public class StoreException : GeneralException
    {
        public StoreException(RiakResult result)
            : base(result.ErrorMessage, result.ResultCode)
        {
        }
    }
}
