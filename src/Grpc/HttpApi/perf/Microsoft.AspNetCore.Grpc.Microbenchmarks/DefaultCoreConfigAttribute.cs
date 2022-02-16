// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BenchmarkDotNet.Configs;

namespace Microsoft.AspNetCore.Grpc.Microbenchmarks;

[AttributeUsage(AttributeTargets.Assembly)]
public class DefaultCoreConfigAttribute : Attribute, IConfigSource
{
    public IConfig Config => new DefaultCoreConfig();
}
