using InsurancePortal.QuoteService.Application.Abstractions.Cqrs;

namespace InsurancePortal.QuoteService.Infrastructure.Cqrs
{
    /// <summary>
    /// Resolves the registered handler for a command or query from the DI container and invokes it
    /// </summary>
    public class Dispatcher : IDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public Dispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public Task<TResult> SendAsync<TCommand, TResult>(TCommand command) where TCommand : ICommand<TResult>
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var handler = _serviceProvider.GetService(typeof(ICommandHandler<TCommand, TResult>)) as ICommandHandler<TCommand, TResult>
                ?? throw new InvalidOperationException($"No handler registered for command {typeof(TCommand).Name}");

            return handler.HandleAsync(command);
        }

        public Task<TResult> QueryAsync<TQuery, TResult>(TQuery query) where TQuery : IQuery<TResult>
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var handler = _serviceProvider.GetService(typeof(IQueryHandler<TQuery, TResult>)) as IQueryHandler<TQuery, TResult>
                ?? throw new InvalidOperationException($"No handler registered for query {typeof(TQuery).Name}");

            return handler.HandleAsync(query);
        }
    }
}
