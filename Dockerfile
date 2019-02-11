# To build this file:
#   > docker build -t hyperledger/sawtooth-sdk-dotnet .
#
# To run interactively after build
#   > docker run -it --entrypoint "/bin/bash" hyperledger/sawtooth-sdk-dotnet
#
# In order to run the examples, change to their working directory and run the projects
#   > cd /sawtooth/sdk/examples
#   > dotnet run Processor tcp://127.0.0.1:4004
#   > dotnet run Client mykey set 42

FROM microsoft/dotnet:sdk

WORKDIR /sdk
COPY . .
RUN dotnet restore src/

WORKDIR /core
RUN git clone https://github.com/hyperledger/sawtooth-core.git

ENTRYPOINT find /core/sawtooth-core/protos/ -name '*.proto' -print0 \
    | xargs -r0 ~/.nuget/packages/google.protobuf.tools/3.5.1/tools/linux_x64/protoc \
    --csharp_out=/sdk/src/Protobuf --proto_path=/core/sawtooth-core/protos/ 