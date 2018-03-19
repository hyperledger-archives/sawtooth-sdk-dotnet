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

FROM microsoft/dotnet

COPY . /sawtooth/sdk

EXPOSE 4004 8008

WORKDIR /sawtooth/sdk/src

RUN dotnet restore \
    && /sawtooth/sdk/bin/build_protos \
    && dotnet publish -c Release -f netstandard2.0 Sdk \
    && dotnet pack -o . --runtime netstandard2.0 Sdk \
    && dotnet test Test
