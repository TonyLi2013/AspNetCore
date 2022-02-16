// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Grpc.AspNetCore.Server;
using Grpc.Core;
using Grpc.Shared.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal.CallHandlers;

internal abstract class ServerCallHandlerBase<TService, TRequest, TResponse>
    where TService : class
    where TRequest : class
    where TResponse : class
{
    private const string LoggerName = "Grpc.AspNetCore.Grpc.HttpApi.ServerCallHandler";

    protected ServerMethodInvokerBase<TService, TRequest, TResponse> MethodInvoker { get; }
    public CallHandlerDescriptorInfo DescriptorInfo { get; }
    public JsonSerializerOptions SerializerOptions { get; }
    protected ILogger Logger { get; }

    protected ServerCallHandlerBase(
        ServerMethodInvokerBase<TService, TRequest, TResponse> methodInvoker,
        ILoggerFactory loggerFactory,
        CallHandlerDescriptorInfo descriptorInfo,
        JsonSerializerOptions serializerOptions)
    {
        MethodInvoker = methodInvoker;
        DescriptorInfo = descriptorInfo;
        SerializerOptions = serializerOptions;
        Logger = loggerFactory.CreateLogger(LoggerName);
    }

    public Task HandleCallAsync(HttpContext httpContext)
    {
        var serverCallContext = new HttpApiServerCallContext(httpContext, MethodInvoker.Options, MethodInvoker.Method, DescriptorInfo, Logger);
        httpContext.Features.Set<IServerCallContextFeature>(serverCallContext);

        try
        {
            var handleCallTask = HandleCallAsyncCore(httpContext, serverCallContext);

            if (handleCallTask.IsCompletedSuccessfully)
            {
                return Task.CompletedTask;
            }
            else
            {
                return AwaitHandleCall(serverCallContext, MethodInvoker.Method, IsStreaming, SerializerOptions, handleCallTask);
            }
        }
        catch (Exception ex)
        {
            return serverCallContext.ProcessHandlerErrorAsync(ex, MethodInvoker.Method.Name, IsStreaming, SerializerOptions);
        }

        static async Task AwaitHandleCall(HttpApiServerCallContext serverCallContext, Method<TRequest, TResponse> method, bool isStreaming, JsonSerializerOptions serializerOptions, Task handleCall)
        {
            try
            {
                await handleCall;
            }
            catch (Exception ex)
            {
                await serverCallContext.ProcessHandlerErrorAsync(ex, method.Name, isStreaming, serializerOptions);
            }
        }
    }

    protected abstract Task HandleCallAsyncCore(HttpContext httpContext, HttpApiServerCallContext serverCallContext);

    protected virtual bool IsStreaming => false;
}
