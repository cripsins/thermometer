# thermometer
=============

[![Build status](https://ci.appveyor.com/api/projects/status/0ich9l0ythfbnc2r/branch/master?svg=true)](https://ci.appveyor.com/project/PhilKellner/thermometer/branch/master)

*Thermometer* is a Dotnet Core Middleware library that calculates the minimum, maximum and average time it takes to process requests for the given ASP.NET Core website. The goal of this project to plug-in easily, require few dependencies and not dramatically interrupt the middleware pipeline.

## Getting up and running

The project includes a sample ASP.NET Core WebApp with the middleware already installed at `thermometer.sandbox` to use as reference.

Your ASP.NET Core WebApp must have `Memory Cache` configured, a simple version of this would be to add:

    services.AddMemoryCache();
    
In your `ConfigureServices` call in your `Startup.cs` file.

And finally, you will need to add

    app.UseMiddleware<ThermometerMiddleware>();
    
In the `Configure` call in your `Startup.cs` file, but BEFORE any rendering calls i.e. `app.UseMvc();`

If all goes well you should see some output in all HTML calls to your webserver, like so:

[![Screenshot](https://i.imgur.com/yEdDnPk.png)]

## Building

Once you've cloned this repository, `cd` to `thermometer.sandbox` and do a `dotnet build` which should restore and build all dependencies

## Building

Go to `thermometer.middleware.tests` and run `dotnet test` to run tests! Not many here but more to come I'm sure.

## TODO/Future features

- Create a nice nuget for this project
- Refactor the TemperatureCalculations class, or atleast consider DI in the middleware
- Consider writing to other response types i.e css and js
- Performance profile the WriteAsync call, perhaps a StreamWrapper would be quicker
- Make the temperature data storage flexible and driven by configuration
- Store historic values off-site i.e. push to a queue and publish data to a document store
