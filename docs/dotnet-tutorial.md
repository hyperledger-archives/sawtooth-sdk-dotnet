# Hyperledger Sawtooth with .NET Core
>
[Hyperledger Sawtooth](https://sawtooth.hyperledger.org/docs/core/releases/latest/introduction.html) is an enterprise blockchain platform for building distributed ledger applications and networks. The design philosophy targets keeping ledgers distributed and making smart contracts safe, particularly for enterprise use.
>
Sawtooth simplifies blockchain application development by separating the core system from the application domain. Application developers can specify the business rules appropriate for their application, using the language of their choice, without needing to know the underlying design of the core system.
>
Sawtooth is also highly modular. This modularity enables enterprises and consortia to make policy decisions that they are best equipped to make. Sawtooth’s core design allows applications to choose the transaction rules, permissioning, and consensus algorithms that support their unique business needs.
>
Sawtooth is an open source project under the Hyperledger umbrella. For information on how to contribute, see [Join the Sawtooth Community](https://sawtooth.hyperledger.org/docs/core/releases/latest/introduction.html#join-the-sawtooth-community).
>

---

## Introduction

In this tutorial we will build a sample application for Sawtooth that stores the state of a number to the blockchain. We will use the [Sawtooth SDK for .NET Core](https://www.nuget.org/packages/Sawtooth.Sdk/) available from Nuget. Hyperledger stores information on the blockchain as key value pairs. Each key represents a unique address that points to the value of that object. The value can be anything, it is stored as byte array, so we can serialize any object state using different serialziation methods. For this tutorial, we will use CBOR (Concise Binary Object Representation) serialization using the [PeterO.Cbor](https://www.nuget.org/packages/PeterO.Cbor/) package. You can use JSON or binary serialization for your project if you prefer to do so.

## Prerequisites

- Make sure you have .[NET Core 2.0+](https://www.microsoft.com/net/download/) installed on your local machine
- Any IDE will work, since you can run `dotnet` commands from the command line, however Visual Studio or VS Code work pretty well
- Install [Docker](https://docs.docker.com/install/). We will run Sawtooth in a container. There are many images available from the above repository that we will use.

### Create new project

From the command line, type `dotnet new console -lang c# -n Processor` inside a blank directory. This will create new project for our transaction processor. While you're there, also create a project for our client application. The client application will use the Sawtooth REST API to send transaction requests to the blockchain `dotnet new console -lang c# -n Client`. You can also create new projects using File / New Project from the VS menu.

Let's add the packages that we will use. From within the Processor and Client folder type:

`dotnet add package Sawtooth.Sdk`

`dotnet add package PeterO.Cbor`

### Transaction processor

In Sawtooth, transaction processors are the components that modify the state of the ledger. This is where your blockchain business logic will be placed. Transaction processors process requests coming from the REST API and send state changes to the validators. The validators then decide if this state change will be accepted and become part of the chain.
Creating a transaction processor is easy, the SDK provides two contracts for this.
- TransactionProcessor class - this class is responsible for the communication with the validators. The processor and the validator communicate using ZeroMQ.
- ITransactionHandler interface - we will implement this interface and put our business logic here.

Add the SDK package to the project using `dotnet add package Sawtooth.Sdk`. The purpose of our processor will be to modify an integer stored at a specific address. We have decided that in order to do this, we will the name of our integer and an action to be done (set value, increase or decrease).
Let's implement the `ITransactionHandler` interface.

Create a new `IntKeyHandler.cs` file and implement the handler interface.

~~~cs
public class IntKeyHandler : ITransactionHandler
{
    public string FamilyName => throw new NotImplementedException();

    public string Version => throw new NotImplementedException();

    public string[] Namespaces => throw new NotImplementedException();

    public Task ApplyAsync(TpProcessRequest request, TransactionContext context)
    {
        throw new NotImplementedException();
    }
}
~~~

This is all that's required to create a handler. Let's take a look at each implementation.
The `FamilyName` sets the name of our transaction family. This name along with the `Version` identify our handler with the validator. Set these values to `"intkey"` and `"1.0"`.
The `Namespaces` plays a significant role in how addressing work in Sawtooth. Hyperledger Sawtooth stores data within a Merkle Tree. Data is stored in leaf nodes, and each node is accessed using an addressing scheme that is composed of 35 bytes, represented as 70 hex characters.
~~~cs
public string FamilyName { get => "intkey"; }
public string Version { get => "1.0"; }
public string[] Namespaces { get => new[] { FamilyName.ToByteArray().ToSha512().TakeLast(32).ToArray().ToHexString() }; }
~~~
We will use this helper method to construct the full address for a given key name
~~~cs
readonly string PREFIX = "intkey".ToByteArray().ToSha512().ToHexString().Substring(0, 6);
string GetAddress(string name) => PREFIX + name.ToByteArray().ToSha512().TakeLast(32).ToArray().ToHexString();
~~~
The recommended way to construct an address is to use the hex-encoded hash values of the string or strings that make up the address elements. You can read the documentation on [Address and Namespace Design](https://sawtooth.hyperledger.org/docs/core/releases/latest/app_developers_guide/address_and_namespace.html) for full details. To construct the full will use 6 chars for the namespace and 64 chars for the address.

Next is the implementation of `ApplyAsync` method. This method will be called everytime Sawtooth receives a transaction for this family name and version. Our entire logic will be placed in this method.
In short, this is what our implementation will look like

- Decode the request payload using cbor
- Extract the name and verb (action) values
- Call `SetValue`, `Increase` or `Decrease` methods depending on the action

Add the following code as the method implementation
```cs
public async Task ApplyAsync(TpProcessRequest request, TransactionContext context)
{
    var obj = CBORObject.DecodeFromBytes(request.Payload.ToByteArray());

    var name = obj["Name"].AsString();
    var verb = obj["Verb"].AsString().ToLowerInvariant();

    switch (verb)
    {
        case "set":
            var value = obj["Value"].AsInt32();
            await SetValue(name, value, context);
            break;
        case "inc":
            await Increase(name, context);
            break;
        case "dec":
            await Decrease(name, context);
            break;
        default:
            throw new InvalidTransactionException($"Unknown verb {verb}");
    }
}
```

The `request` parameter will be populated with the payload that was sent from the client and will contain the serialized byte array of our request. We haven't written any data on the ledger yet, this method only accepts the requests and parses the object.

Add the implementation for the `SetValue` method

~~~cs
async Task SetValue(string name, int value, TransactionContext context)
{
    var state = await context.GetStateAsync(Arrayify(GetAddress(name)));
    if (state != null && state.Any() && !state.First().Value.IsEmpty)
    {
        throw new InvalidTransactionException($"Verb is 'set', but address is aleady set");
    }
    await context.SetStateAsync(new Dictionary<string, ByteString>
    {
        { GetAddress(name), ByteString.CopyFrom(BitConverter.GetBytes(value)) }
    });
    Console.WriteLine($"Value for {name} decreased to {value}");
}
~~~

This method does three things in order
- Reads the state for this address from the ledger
- If the state already exists, throws an error (we can only increase or decrease the value once it's set - this is a design choice, not a requirement)
- If the state wasn't set, write the value to the ledger

We interact with the state of the ledger by utilizing the `TransactionContext` class. This class contains methods for retrieving and writing the state of the ledger, namely `GetSatetAsync(address)` and `SetStateAsync([address, value])`. It's important that the address is formed properly, otherwise it will not be written.

The remaining two methods look like this

~~~cs
async Task Decrease(string name, TransactionContext context)
{
    var state = await context.GetStateAsync(Arrayify(GetAddress(name)));
    if (state != null && state.Any() && !state.First().Value.IsEmpty)
    {
        var val = BitConverter.ToInt32(state.First().Value.ToByteArray(), 0) - 1;
        await context.SetStateAsync(new Dictionary<string, ByteString>
        {
            { state.First().Key, ByteString.CopyFrom(BitConverter.GetBytes(val)) }
        });
        Console.WriteLine($"Value for {name} decreased to {val}");
        return;
    }
    throw new InvalidTransactionException($"Verb is 'dec', but state wasn't found at this address");
}

async Task Increase(string name, TransactionContext context)
{
    var state = await context.GetStateAsync(Arrayify(GetAddress(name)));
    if (state != null && state.Any() && !state.First().Value.IsEmpty)
    {
        var val = BitConverter.ToInt32(state.First().Value.ToByteArray(), 0) + 1;
        await context.SetStateAsync(new Dictionary<string, ByteString>
        {
            { state.First().Key, ByteString.CopyFrom(BitConverter.GetBytes(val)) }
        });
        Console.WriteLine($"Value for {name} increased to {val}");
        return;
    }
    throw new InvalidTransactionException("Verb is 'inc', but state wasn't found at this address");
}
~~~

Always make sure to throw an `InvalidTransactionException` to indicate that this request was invalid and the transaction should be set to invalid.

And finally, in our `Program.cs` file we can instantiate and run this processor:

~~~cs
static void Main(string[] args)
{
    var validatorAddress = args.Any() ? args.First() : "tcp://127.0.0.1:4004";

    if (!Uri.TryCreate(validatorAddress, UriKind.Absolute, out var _)) 
        throw new Exception($"Invalid validator address: {validatorAddress}");

    var processor = new TransactionProcessor(validatorAddress);
    processor.AddHandler(new IntKeyProcessor());
    processor.Start();

    Console.CancelKeyPress += delegate { processor.Stop(); };
}
~~~

That's all for the processor code. Creating transaction processors with Sawtooth isn't difficult and you have the choice to run your processor in any language. There are a number of SDKs available for this.

Once you run the above program it will connect to a running instance of the validator and start listening for incoming messages. The validator by default is set to run on port 4004.

### Client caller

Next, let's create a small program that will help us send transaction requests so we can interact with the processor and write state to the ledger. Sawtooth provides a REST API for this.
The API is [very well documented](https://sawtooth.hyperledger.org/docs/core/releases/latest/rest_api/endpoint_specs.html), I won't go into the details here. We will be using the `POST /batches` endpoint to send a transaction requests.

Let's add this code to the `Program.cs` file inside the second console project we created earlier, called `Client`.

~~~cs
static void Main(string[] args)
{
    if (args != null && (args.Count() < 2 || args.Count() > 3))
    {
        Console.WriteLine("Name and Verb arguments must be set.", Color.Red);
        Console.WriteLine("Usage: dotnet run [keyname] [verb] [optional value]");
        Console.WriteLine(" dotnet run intkey set 42 \t- sets initial value");
        Console.WriteLine(" dotnet run intkey inc \t- increases existing value");
        Console.WriteLine(" dotnet run intkey dec \t- decreases existing value");
        return;
    }

    var name = args[0];
    var verb = args[1];

    var obj = CBORObject.NewMap()
                        .Add("Name", name)
                        .Add("Verb", verb);
    if (args.Count() == 3)
    {
        obj.Add("Value", Int32.Parse(args[2]));
    }

    var prefix = "intkey".ToByteArray().ToSha512().ToHexString().Substring(0, 6);
    var signer = new Signer();

    var settings = new EncoderSettings()
    {
        BatcherPublicKey = signer.GetPublicKey().ToHexString(),
        SignerPublickey = signer.GetPublicKey().ToHexString(),
        FamilyName = "intkey",
        FamilyVersion = "1.0"
    };
    settings.Inputs.Add(prefix);
    settings.Outputs.Add(prefix);
    var encoder = new Encoder(settings, signer.GetPrivateKey());

    var payload = encoder.EncodeSingleTransaction(obj.EncodeToBytes());

    var content = new ByteArrayContent(payload);
    content.Headers.Add("Content-Type", "application/octet-stream");

    var httpClient = new HttpClient();

    var response = httpClient.PostAsync("http://localhost:8008/batches", content).Result;
    Console.WriteLine(response.Content.ReadAsStringAsync().Result);
}
~~~

This program forms a request that our processor can understand. In order to do that, we will create a Cbor encoded object and add the proper Name, Verb and Value fields accordingly.
This payload is then encoded in a transaction and the entire data is sent to the endpoint.

The SDK provides `Encoder` class with helper functions to create transactions and batches. All transactions are wrapped inside a batch. Both the batch and the transactions must be signed.
We can use the `Signer` class to create a key pair and sign the data with the private key. You can use any library to create and sign the headers as long as it's elliptic curve secp256k1. Azure KeyVault can be used as a signer for this puprose, too.

The [Transactions and Batches](https://sawtooth.hyperledger.org/docs/core/releases/latest/architecture/transactions_and_batches.html) documentation has full details on properly formatting the request. You can also check the .NET SDK source code for sample implementation.

Let's make sure both console application build properly. Build the projects from VS or from console

`dotnet build Processor`

`dotnet build Client`

### Run Sawtooth instance with Docker

We will use the tool `docker-compose` that comes with Docker installation. [Download this docker compose file](https://github.com/tmarkovski/sawtooth-sdk-net/blob/master/sawtooth-default.yaml). The file will build docker images and all components needed to run a node of Sawtooth. Run the following from the console:

`docker-compose -f sawtooth-default.yaml up`

After everything is build, docker compose will start all containers. You should see what's going on in your terminal and here you will see when your transaction processor is connected and when trasnactions are processed.
You can also check out the documentation on running [Sawtooth with Docker](https://sawtooth.hyperledger.org/docs/core/releases/latest/app_developers_guide/docker.html?highlight=docker).

Once everything completes successfully, we will start our Processor.
From Visual Studio run the project Processor, or run this in console from within the Processor folder:

`dotnet run`

You can also run `dotnet run tcp://[host_address]:4004` if your processor runs on a different host. Sawtooth validator must already be running for the processor to connect to.

You should see a message that the processor was connected and registered successfully. The processor is now listening for incoming transaction requests.
Next, we will run the Client to create transaction requests and the processor will act on this.
From a console, change into the Client project folder and type

`dotnet run mykey set 42`

This will send a message to the REST API which will direct it to our processor. You should see a message with a link that you can paste in browser. This link will give the transaction status.
In the Processor console, you should see a message that the value for key `mykey` has been set to 42.

Run some more commands from the Client.

`dotnet run mykey inc` to increase the value of `mykey`

or

`dotnet run mykey dec` to decrease it.

Congratulations! You've written a full end to end Hyperledger Sawtooth application.

Please feel free to reach out with any questions or issues you find with the .NET SDK.
Hope you enjoyed reading this, happy coding!

Published article at https://tomislav.tech/2018-03-02-sawtooth-sdk-net-core/