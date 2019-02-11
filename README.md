# Sawtooth SDK for .NET Core

## Installation

The SDK is available from [Nuget](https://www.nuget.org/packages/Sawtooth.Sdk/) built for netstandard2.0.

```
dotnet add package Sawtooth.Sdk
```

## Tutorial
Check the [docs](docs/) folder for an example transaction processor and client. Sample code is available in [examples](examples/) folder.

## Building the package

In order to build the SDK we need to generate the protobuf files in `src/Protobuf` folder. These files are generated from the .proto definitions at https://github.com/hyperledger/sawtooth-core/tree/master/protos
You can either do this manually, or follow the instructions below.

### Prerequisites

- Install [Docker](https://www.docker.com/products/docker-desktop)
- Install [.NET Core](https://dotnet.microsoft.com/download)

### Build steps

Clone this repo 
```
git clone https://github.com/hyperledger/sawtooth-sdk-dotnet.git
```
Change directory 
```
cd sawtooth-sdk-dotnet
```
Build docker image
```
docker build -t sawtooth-sdk-dotnet .
```
Run the image to generate the files in `src/Protobuf`
```
docker run -v ${PWD}/src/Protobuf:/sdk/src/Protobuf sawtooth-sdk-dotnet
```
Finally build your solution
```
dotnet build src
```