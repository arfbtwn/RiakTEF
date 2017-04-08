using System.Threading.Tasks;
using RiakClient;
using RiakClient.Commands;
using RiakClient.Commands.TS;

namespace RiakTEF.Low
{
    class Database : IDatabase
    {
        public IRiakClient Client { get; }

        public Database(IRiakClient client)
        {
            Client = client;
        }

        public RiakResult Query(Query command)
        {
            return new Q(Client, command).Execute();
        }

        public Task<RiakResult> QueryAsync(Query command)
        {
            return new Q(Client, command).ExecuteAsync();
        }

        public RiakResult Store(Store command)
        {
            return new S(Client, command).Execute();
        }

        public Task<RiakResult> StoreAsync(Store command)
        {
            return new S(Client, command).ExecuteAsync();
        }

        public RiakResult Execute(IRiakCommand command)
        {
            return new E(Client, command).Execute();
        }

        public Task<RiakResult> ExecuteAsync(IRiakCommand command)
        {
            return new E(Client, command).ExecuteAsync();
        }
    }

    class E
    {
        readonly IRiakClient  _client;
        readonly IRiakCommand _command;

        public E(IRiakClient client, IRiakCommand command)
        {
            _client  = client;
            _command = command;
        }

        protected virtual RiakResult Assert(RiakResult result)
        {
            Assertion.IsSuccess(result);
            return result;
        }

        protected RiakResult Assert(Task<RiakResult> result)
        {
            return Assert(result.Result);
        }

        public virtual RiakResult Execute()
        {
            return Assert(_client.Execute(_command));
        }

        public virtual Task<RiakResult> ExecuteAsync()
        {
            return _client.Async.Execute(_command).ContinueWith(Assert);
        }
    }

    class Q : E
    {
        readonly Query _command;

        public Q(IRiakClient client, Query command) : base(client, command)
        {
            _command = command;
        }

        protected override RiakResult Assert(RiakResult result)
        {
            Assertion.IsSuccess(_command, result);

            return result;
        }
    }

    class S : E
    {
        readonly Store _command;

        public S(IRiakClient client, Store command) : base(client, command)
        {
            _command = command;
        }

        protected override RiakResult Assert(RiakResult result)
        {
            Assertion.IsSuccess(_command, result);

            return result;
        }
    }
}
