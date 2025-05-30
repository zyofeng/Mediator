using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;
using static Mediator.Tests.OpenConstrainedGenericsTests;

namespace Mediator.Tests;

public class BasicHandlerTests
{
    [Fact]
    public void Test_Initialization()
    {
        var (_, mediator) = Fixture.GetMediator();
        Assert.NotNull(mediator);
    }

    [Fact]
    public void Test_Get_Handlers()
    {
        var (sp, _) = Fixture.GetMediator();

        INotificationHandler<SomeStructNotification> handler1 = new SomeStructNotificationHandler();
        //INotificationHandler<SomeStructNotification> handler2 = new CatchAllPolymorphicNotificationHandler();
        INotificationHandler<SomeStructNotification> handler3 =
            new SomeGenericConstrainedNotificationHandler<SomeStructNotification>();

        var handlers = sp.GetServices<INotificationHandler<SomeStructNotification>>();
        Assert.NotNull(handlers);
        var handlersArray = handlers.ToArray();
        Assert.NotNull(handlersArray);
    }

    [Fact]
    public async Task Test_Request_Handler()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var response = await mediator.Send(new SomeRequest(id));
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        response = await mediator.Send((object)new SomeRequest(id)) as SomeResponse;
        Assert.NotNull(response);
        Assert.Equal(id, response?.Id);
        response = await concrete.Send(new SomeRequest(id));
        Assert.NotNull(response);
        Assert.Equal(id, response?.Id);
    }

    [Fact]
    public async Task Test_Request_Handler_Null_Input()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Send((IRequest<SomeResponse>)null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Send(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await concrete.Send((SomeRequest)null!));
    }

    [Fact]
    public async Task Test_Request_Handler_NonNull_NonRequest()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        object message = new { Id = id };

        await Assert.ThrowsAsync<InvalidMessageException>(async () => await mediator.Send(message));
    }

    [Fact]
    public async Task Test_Request_NonNull_NoHandler()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var request = new SomeRequestWithoutHandler(id);

        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () => await mediator.Send((object)request));
        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () => await mediator.Send(request));
        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () => await concrete.Send(request));
    }

    [Fact]
    public async Task Test_RequestWithoutResponse_Handler()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var requestHandler = sp.GetRequiredService<SomeRequestWithoutResponseHandler>();
        await mediator!.Send(new SomeRequestWithoutResponse(id));
        Assert.NotNull(requestHandler);
        Assert.Contains(id, SomeRequestWithoutResponseHandler.Ids);
    }

    [Fact]
    public async Task Test_Query_Handler()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var response = await mediator.Send(new SomeQuery(id));
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        response = await mediator.Send((object)new SomeQuery(id)) as SomeResponse;
        Assert.NotNull(response);
        Assert.Equal(id, response?.Id);
        response = await concrete.Send(new SomeQuery(id));
        Assert.NotNull(response);
        Assert.Equal(id, response?.Id);
    }

    [Fact]
    public async Task Test_Query_Handler_Null_Input()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Send((IQuery<SomeResponse>)null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Send(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await concrete.Send((SomeQuery)null!));
    }

    [Fact]
    public async Task Test_Query_Handler_NonNull_NonQuery()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        object message = new { Id = id };

        await Assert.ThrowsAsync<InvalidMessageException>(async () => await mediator.Send(message));
    }

    [Fact]
    public async Task Test_Query_NonNull_NoHandler()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var query = new SomeQueryWithoutHandler(id);

        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () => await mediator.Send((object)query));
        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () => await mediator.Send(query));
        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () => await concrete.Send(query));
    }

    [Fact]
    public async Task Test_Command_Handler()
    {
        var (sp, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var commandHandler = sp.GetRequiredService<SomeCommandHandler>();
        var response = await mediator.Send(new SomeCommand(id));
        Assert.NotNull(commandHandler);
        Assert.Contains(id, SomeCommandHandler.Ids);
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        response = await mediator.Send((object)new SomeCommand(id)) as SomeResponse;
        Assert.Contains(id, SomeCommandHandler.Ids);
        Assert.NotNull(response);
        Assert.Equal(id, response?.Id);
        response = await concrete.Send(new SomeCommand(id));
        Assert.Contains(id, SomeCommandHandler.Ids);
        Assert.NotNull(response);
        Assert.Equal(id, response?.Id);
    }

    [Fact]
    public async Task Test_Command_Handler_Null_Input()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Send((ICommand<SomeResponse>)null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Send(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await concrete.Send((SomeCommand)null!));
    }

    [Fact]
    public async Task Test_Command_Handler_NonNull_NonCommand()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        object message = new { Id = id };

        await Assert.ThrowsAsync<InvalidMessageException>(async () => await mediator.Send(message));
    }

    [Fact]
    public async Task Test_Command_NonNull_NoHandler()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var command = new SomeCommandWithoutHandler(id);

        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () => await mediator.Send((object)command));
        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () => await mediator.Send(command));
        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () => await concrete.Send(command));
    }

    [Fact]
    public async Task Test_CommandWithoutResponse_Handler()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var commandHandler = sp.GetRequiredService<SomeCommandWithoutResponseHandler>();
        Assert.NotNull(commandHandler);
        await mediator.Send(new SomeCommandWithoutResponse(id));
        Assert.Contains(id, SomeCommandWithoutResponseHandler.Ids);
    }

    [Fact]
    public unsafe void Test_StructCommand_Handler()
    {
        var (sp, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();
        var command = new SomeStructCommand(id);
        var addr = *(long*)&command;

        var commandHandler = sp.GetRequiredService<SomeStructCommandHandler>();
        Assert.NotNull(commandHandler);
#pragma warning disable xUnit1031
        mediator.Send(command).GetAwaiter().GetResult();
#pragma warning restore xUnit1031
        Assert.Contains(id, SomeStructCommandHandler.Ids);
        Assert.Contains(addr, SomeStructCommandHandler.Addresses);
    }

    [Fact]
    public unsafe void Test_StructCommand_Handler_Concrete()
    {
        var (sp, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();
        var command = new SomeStructCommand(id);
        var addr = *(long*)&command;

        var commandHandler = sp.GetRequiredService<SomeStructCommandHandler>();
#pragma warning disable xUnit1031
        concrete.Send(command).GetAwaiter().GetResult();
#pragma warning restore xUnit1031
        Assert.Contains(id, SomeStructCommandHandler.Ids);
        Assert.Contains(addr, SomeStructCommandHandler.Addresses);
    }

    [Fact]
    public async Task Test_Notification_Handler()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var notificationHandler = sp.GetRequiredService<SomeNotificationHandler>();
        Assert.NotNull(notificationHandler);
        await mediator.Publish(new SomeNotification(id));
        Assert.Contains(id, SomeNotificationHandler.Ids);
        AssertInstanceIdCount(1, notificationHandler.InstanceIds, id);

        var handlers = sp.GetServices<INotificationHandler<SomeNotification>>();
        Assert.True(handlers.Distinct().Count() == handlers.Count());
    }

    [Fact]
    public async Task Test_Struct_Notification_Handler()
    {
        var (sp, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();
        var notification = new SomeStructNotification(id);

        var handlers = sp.GetServices<INotificationHandler<SomeStructNotification>>();
        Assert.True(handlers.Count() == 2);
        Assert.True(handlers.Distinct().Count() == handlers.Count());

        var notificationHandler = sp.GetRequiredService<SomeStructNotificationHandler>();
        Assert.NotNull(notificationHandler);

        await concrete.Publish(notification);
        Assert.Contains(id, SomeStructNotificationHandler.Ids);
        AssertInstanceIdCount(1, notificationHandler.InstanceIds, id);
    }

    [Fact]
    public async Task Test_INotification_Handler()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var notificationHandler = sp.GetRequiredService<SomeNotificationHandler>();
        Assert.NotNull(notificationHandler);
        await mediator.Publish<INotification>(new SomeNotification(id));
        Assert.Contains(id, SomeNotificationHandler.Ids);
        AssertInstanceIdCount(1, notificationHandler.InstanceIds, id);

        var handlers = sp.GetServices<INotificationHandler<SomeNotification>>();
        Assert.True(handlers.Distinct().Count() == handlers.Count());
    }

    [Fact]
    public async Task Test_Notification_Handler_Null_Input()
    {
        var (sp, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var notificationHandler = sp.GetRequiredService<SomeNotificationHandler>();
        Assert.NotNull(notificationHandler);
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Publish<INotification>(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Publish<SomeNotification>(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Publish(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await concrete.Publish((SomeNotification)null!));
        Assert.DoesNotContain(id, SomeNotificationHandler.Ids);
        AssertInstanceIdCount(0, notificationHandler.InstanceIds, id);
    }

    [Fact]
    public async Task Test_Notification_Handler_NonNull_NonNotification()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        object message = new { Id = id };

        await Assert.ThrowsAsync<InvalidMessageException>(async () => await mediator.Publish(message));
    }

    [Fact]
    public async Task Test_Multiple_Notification_Handlers()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var handler1 = sp.GetRequiredService<SomeNotificationHandler>();
        var handler2 = sp.GetRequiredService<SomeOtherNotificationHandler>();
        Assert.NotNull(handler1);
        Assert.NotNull(handler2);
        await mediator.Publish(new SomeNotification(id));
        await mediator.Publish((object)new SomeNotification(id));
        Assert.Contains(id, SomeNotificationHandler.Ids);
        Assert.Contains(id, SomeOtherNotificationHandler.Ids);
        AssertInstanceIdCount(2, handler1.InstanceIds, id);
        AssertInstanceIdCount(2, handler2.InstanceIds, id);
    }

    [Fact]
    public async Task Test_Multiple_Object_Notification_Handlers()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var handler1 = sp.GetRequiredService<SomeNotificationHandler>();
        var handler2 = sp.GetRequiredService<SomeOtherNotificationHandler>();
        Assert.NotNull(handler1);
        Assert.NotNull(handler2);
        await mediator.Publish((object)new SomeNotification(id));
        Assert.Contains(id, SomeNotificationHandler.Ids);
        Assert.Contains(id, SomeOtherNotificationHandler.Ids);
        AssertInstanceIdCount(1, handler1.InstanceIds, id);
        AssertInstanceIdCount(1, handler2.InstanceIds, id);
    }

    [Fact]
    public async Task Test_Static_Nested_Request_Handler()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var handler = sp.GetRequiredService<SomeStaticClass.SomeStaticNestedHandler>();
        var response = await mediator!.Send(new SomeStaticClass.SomeStaticNestedRequest(id));
        Assert.NotNull(handler);
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Contains(id, SomeStaticClass.SomeStaticNestedHandler.Ids);
    }

    [Fact]
    public async Task Test_Nested_Request_Handler()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var handler = sp.GetRequiredService<SomeOtherClass.SomeNestedHandler>();
        var response = await mediator!.Send(new SomeOtherClass.SomeNestedRequest(id));
        Assert.NotNull(handler);
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Contains(id, SomeOtherClass.SomeNestedHandler.Ids);
    }

    [Fact]
    public async Task Test_Request_Returning_Array()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();
        var bytes = id.ToByteArray();

        var request = new SomeRequestReturningByteArray(id);
        var receivedBytes = await mediator.Send(request);
        Assert.Equal(bytes, receivedBytes);
    }

    [Fact]
    public async Task Test_Remove_NotificationHandler()
    {
        var (sp, mediator) = Fixture.GetMediator(sc =>
        {
            var sds = sc.Where(static a =>
                a.ImplementationFactory is { } implFac
                && implFac.Method.IsGenericMethod
                && implFac.Method.GetGenericArguments().Any(static b => b.Name == nameof(SomeNotificationHandler))
            );
            var sd = Assert.Single(sds);
            sc.Remove(sd);
        });

        var id = Guid.NewGuid();

        var handler1 = sp.GetRequiredService<SomeNotificationHandler>();
        var handler2 = sp.GetRequiredService<SomeOtherNotificationHandler>();
        Assert.NotNull(handler1);
        Assert.NotNull(handler2);
        await mediator.Publish(new SomeNotification(id));
        await mediator.Publish((object)new SomeNotification(id));
        Assert.DoesNotContain(id, SomeNotificationHandler.Ids);
        Assert.Contains(id, SomeOtherNotificationHandler.Ids);
        AssertInstanceIdCount(0, handler1.InstanceIds, id);
        AssertInstanceIdCount(2, handler2.InstanceIds, id);
    }
}
