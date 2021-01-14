﻿using Grains;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Statistics;
using System.Threading.Tasks;
using Amazon.Util.Internal;
using OrleansDashboard;

namespace SiloHost
{
    class Program
    {
        public static Task Main()
        {

            return new HostBuilder()
                .UseOrleans(siloBuilder =>
                {
                    IEnvironmentVariables environmentVariablesService = new EnvironmentVariables();
                    siloBuilder.UseLinuxEnvironmentStatistics();
                    siloBuilder.ConfigureDashboardOptions(environmentVariablesService);
                    //Register silo with dynamo cluster
                    siloBuilder.ConfigureDynamoClusterOptions(environmentVariablesService);
                    siloBuilder.ConfigureClusterOptions();
                    siloBuilder.ConfigureEndpointOptions(environmentVariablesService);
                    siloBuilder.ConfigureApplicationParts(applicationPartManager =>
                        applicationPartManager.AddApplicationPart(typeof(HelloWorld).Assembly).WithReferences());

                    /*Registering Feature Management, to allow DI of IFeatureManagerSnapshot in HelloWorld grain.
                     Using built in Percentage filter to demonstrate a feature being on/off.*/
                    siloBuilder.ConfigureServices(serviceCollection =>
                    {
                        serviceCollection.AddFeatureManagement()
                            .AddFeatureFilter<PercentageFilter>();
                    });
                })
                .ConfigureLogging(logging => logging.AddConsole())

                //Registering a Configuration source for Feature Management.
                .ConfigureAppConfiguration(config => { config.AddJsonFile("appsettings.json"); })
                .RunConsoleAsync();
        }
    }
}