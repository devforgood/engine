// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: lobby.proto
// </auto-generated>
// Original file comments:
// Copyright 2015 gRPC authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#pragma warning disable 0414, 1591
#region Designer generated code

using grpc = global::Grpc.Core;

namespace GameService {
  /// <summary>
  /// The greeting service definition.
  /// </summary>
  public static partial class Lobby
  {
    static readonly string __ServiceName = "GameService.Lobby";

    static readonly grpc::Marshaller<global::GameService.HelloRequest> __Marshaller_GameService_HelloRequest = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::GameService.HelloRequest.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::GameService.HelloReply> __Marshaller_GameService_HelloReply = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::GameService.HelloReply.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::GameService.LoginRequest> __Marshaller_GameService_LoginRequest = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::GameService.LoginRequest.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::GameService.LoginReply> __Marshaller_GameService_LoginReply = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::GameService.LoginReply.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::GameService.StartPlayRequest> __Marshaller_GameService_StartPlayRequest = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::GameService.StartPlayRequest.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::GameService.StartPlayReply> __Marshaller_GameService_StartPlayReply = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::GameService.StartPlayReply.Parser.ParseFrom);

    static readonly grpc::Method<global::GameService.HelloRequest, global::GameService.HelloReply> __Method_SayHello = new grpc::Method<global::GameService.HelloRequest, global::GameService.HelloReply>(
        grpc::MethodType.Unary,
        __ServiceName,
        "SayHello",
        __Marshaller_GameService_HelloRequest,
        __Marshaller_GameService_HelloReply);

    static readonly grpc::Method<global::GameService.LoginRequest, global::GameService.LoginReply> __Method_Login = new grpc::Method<global::GameService.LoginRequest, global::GameService.LoginReply>(
        grpc::MethodType.ServerStreaming,
        __ServiceName,
        "Login",
        __Marshaller_GameService_LoginRequest,
        __Marshaller_GameService_LoginReply);

    static readonly grpc::Method<global::GameService.StartPlayRequest, global::GameService.StartPlayReply> __Method_StartPlay = new grpc::Method<global::GameService.StartPlayRequest, global::GameService.StartPlayReply>(
        grpc::MethodType.ServerStreaming,
        __ServiceName,
        "StartPlay",
        __Marshaller_GameService_StartPlayRequest,
        __Marshaller_GameService_StartPlayReply);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::GameService.LobbyReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of Lobby</summary>
    [grpc::BindServiceMethod(typeof(Lobby), "BindService")]
    public abstract partial class LobbyBase
    {
      /// <summary>
      /// Sends a greeting
      /// </summary>
      /// <param name="request">The request received from the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>The response to send back to the client (wrapped by a task).</returns>
      public virtual global::System.Threading.Tasks.Task<global::GameService.HelloReply> SayHello(global::GameService.HelloRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task Login(global::GameService.LoginRequest request, grpc::IServerStreamWriter<global::GameService.LoginReply> responseStream, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task StartPlay(global::GameService.StartPlayRequest request, grpc::IServerStreamWriter<global::GameService.StartPlayReply> responseStream, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Client for Lobby</summary>
    public partial class LobbyClient : grpc::ClientBase<LobbyClient>
    {
      /// <summary>Creates a new client for Lobby</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      public LobbyClient(grpc::Channel channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for Lobby that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      public LobbyClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      protected LobbyClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      protected LobbyClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      /// <summary>
      /// Sends a greeting
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The response received from the server.</returns>
      public virtual global::GameService.HelloReply SayHello(global::GameService.HelloRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return SayHello(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      /// Sends a greeting
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The response received from the server.</returns>
      public virtual global::GameService.HelloReply SayHello(global::GameService.HelloRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_SayHello, null, options, request);
      }
      /// <summary>
      /// Sends a greeting
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncUnaryCall<global::GameService.HelloReply> SayHelloAsync(global::GameService.HelloRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return SayHelloAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      /// Sends a greeting
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncUnaryCall<global::GameService.HelloReply> SayHelloAsync(global::GameService.HelloRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_SayHello, null, options, request);
      }
      public virtual grpc::AsyncServerStreamingCall<global::GameService.LoginReply> Login(global::GameService.LoginRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Login(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncServerStreamingCall<global::GameService.LoginReply> Login(global::GameService.LoginRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncServerStreamingCall(__Method_Login, null, options, request);
      }
      public virtual grpc::AsyncServerStreamingCall<global::GameService.StartPlayReply> StartPlay(global::GameService.StartPlayRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return StartPlay(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncServerStreamingCall<global::GameService.StartPlayReply> StartPlay(global::GameService.StartPlayRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncServerStreamingCall(__Method_StartPlay, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      protected override LobbyClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new LobbyClient(configuration);
      }
    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static grpc::ServerServiceDefinition BindService(LobbyBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_SayHello, serviceImpl.SayHello)
          .AddMethod(__Method_Login, serviceImpl.Login)
          .AddMethod(__Method_StartPlay, serviceImpl.StartPlay).Build();
    }

    /// <summary>Register service method with a service binder with or without implementation. Useful when customizing the  service binding logic.
    /// Note: this method is part of an experimental API that can change or be removed without any prior notice.</summary>
    /// <param name="serviceBinder">Service methods will be bound by calling <c>AddMethod</c> on this object.</param>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static void BindService(grpc::ServiceBinderBase serviceBinder, LobbyBase serviceImpl)
    {
      serviceBinder.AddMethod(__Method_SayHello, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::GameService.HelloRequest, global::GameService.HelloReply>(serviceImpl.SayHello));
      serviceBinder.AddMethod(__Method_Login, serviceImpl == null ? null : new grpc::ServerStreamingServerMethod<global::GameService.LoginRequest, global::GameService.LoginReply>(serviceImpl.Login));
      serviceBinder.AddMethod(__Method_StartPlay, serviceImpl == null ? null : new grpc::ServerStreamingServerMethod<global::GameService.StartPlayRequest, global::GameService.StartPlayReply>(serviceImpl.StartPlay));
    }

  }
}
#endregion
