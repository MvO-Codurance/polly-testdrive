# Polly Test Drive
Test driving the features of Polly (https://www.thepollyproject.org)

- Open and build the solution
- Open a command prompt in the repo root
- Execute the following to run the web api:

`dotnet run --project ./PollyTestDrive.WebApi/PollyTestDrive.WebApi.csproj`

- Run the unit tests
- View the output for each test in the unit test runner window

## Custom Policies
The `Polly.Contrib.*` projects contain copies of custom policy samples taken from [Polly-Contrib](https://github.com/Polly-Contrib).
These are:

- https://github.com/Polly-Contrib/Polly.Contrib.TimingPolicy
- https://github.com/Polly-Contrib/Polly.Contrib.LoggingPolicy

See also the blog series on 
[custom polices](http://www.thepollyproject.org/2019/02/13/introducing-custom-polly-policies-and-polly-contrib-custom-policies-part-i/).
