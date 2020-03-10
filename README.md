## Hydra
 
 ![Build Status](https://dev.azure.com/varunit/Platform/_apis/build/status/Build%20and%20Package%20Hydra) [![NuGet](https://img.shields.io/nuget/v/Surveily.Hydra.Core.svg)](https://www.nuget.org/packages/Surveily.Hydra.Core/)  [![NuGet](https://img.shields.io/nuget/v/Surveily.Hydra.Events.svg)](https://www.nuget.org/packages/Surveily.Hydra.Events/)

A set of components to take the most advantage of performance and capacity of Azure Storage. 

Hydra is Azure Subscription agnostic, which means it is possible to use Storage Accounts from different Azure Subscriptions. This functionality gives the developer configurable IOPS and Disk Space with no upper limits.

## Overview

![Link](https://github.com/Surveily/Hydra/blob/master/doc/architecture.png)

## Hydra.Core

` class Hydra : IHydra `

A central component for scaling across multiple Storage Accounts. It is using an ISharding strategy to compute consistent hashes that pick a right Storage Account by key provided.

` class JumpSharding : ISharding `

Default implementation of ISharding provided is JumpSharding that implement's Jump Consistent Hash.

#### Disclaimer

Hydra.Core doesn't manage shard migration, which means you are constrained the amount of Storage Accounts you start of with. The more the better.

#### Advanced usage

It is possible to have multiple instances of Hydra, configured to point at different and/or the same Storage Accounts, with different and/or the same ISharding implementations. That feature gives the developer maximum flexibility for making sure the right data is distributed in the right way.

#### Example

Example usage can be found in the Hydra.Tests.Integration namespace.

## Hydra.Events

` class StreamContainer : IStreamContainer `

A central component for managing Stream's underlying storage. It requires an IHydra component to gain access to the storage.

` class Stream : IStream `

This component is in charge of writing and reading events to a stream in storage.

#### Disclaimer

Hydra.Events has a limitation dictated by Azure Storage. Currently one stream can consist of up to 50,000 events and 195GB of space.

#### Example

Example usage can be found in the Hydra.Tests.Integration namespace.

## Hydra.Tools

```
$ hydra
Surveily.Hydra.Tools 1.0.0
Copyright © Surveily sp. z o.o.

ERROR(S):
  No verb selected.

  cp         CAREFUL! If you specify multiple sources or targets, you will use Hydra Jump Sharding. Otherwise simple copy.

  rm         Remove all objects from target storage accounts

  help       Display more information on a specific command.

  version    Display version information.
```
