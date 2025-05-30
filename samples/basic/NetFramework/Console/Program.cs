using System;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleConsole
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var services = new ServiceCollection();

            // This extensions method is generated, and is put in the "Microsoft.Extensions.DependencyInjection" namespace.
            // We override the namespace in the "MediatorOptions" object.
            services.AddMediator(options =>
            {
                options.Namespace = "SimpleConsole";
            });

            // Standard handlers are added by default, but we need to add pipeline steps manually.
            // Here are two examples.
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(GenericLoggerHandler<,>)); // This will run 1st
            services.AddSingleton<IPipelineBehavior<Ping, Pong>, PingValidator>(); // This will run 2nd

            var serviceProvider = services.BuildServiceProvider();

            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var id = Guid.NewGuid();
            var request = new Ping(id);

            var response = await mediator.Send(request);

            Console.WriteLine("-----------------------------------");
            Console.WriteLine("ID: " + id);
            Console.WriteLine(request);
            Console.WriteLine(response);

            return response.Id == id ? 0 : 1;
        }
    }

    //
    // Here are the types used
    //

    public sealed class Ping : IRequest<Pong>
    {
        public Guid Id { get; }

        public Ping(Guid id)
        {
            Id = id;
        }
    }

    public sealed record Pong
    {
        public Guid Id { get; }

        public Pong(Guid id)
        {
            Id = id;
        }
    }

    public sealed class GenericLoggerHandler<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
        where TMessage : IMessage
    {
        public async ValueTask<TResponse> Handle(
            TMessage message,
            MessageHandlerDelegate<TMessage, TResponse> next,
            CancellationToken cancellationToken
        )
        {
            Console.WriteLine("1) Running logger handler");
            try
            {
                var response = await next(message, cancellationToken);
                Console.WriteLine("5) No error!");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }

    public sealed class PingValidator : IPipelineBehavior<Ping, Pong>
    {
        public ValueTask<Pong> Handle(
            Ping request,
            MessageHandlerDelegate<Ping, Pong> next,
            CancellationToken cancellationToken
        )
        {
            Console.WriteLine("2) Running ping validator");
            if (request is null || request.Id == default)
                throw new ArgumentException("Invalid input");
            else
                Console.WriteLine("3) Valid input!");

            return next(request, cancellationToken);
        }
    }

    public sealed class PingHandler : IRequestHandler<Ping, Pong>
    {
        public ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken)
        {
            Console.WriteLine("4) Returning pong!");
            return new ValueTask<Pong>(new Pong(request.Id));
        }
    }
}
