﻿
using Polly;
using System;
using System.Net;
using System.Reflection;
using System.Threading;

namespace PollyTestClient.Samples
{
    /// <summary>
    /// Demonstrates the WaitAndRetry policy.
    /// Loops through a series of Http requests, keeping track of each requested
    /// item and reporting server failures when encountering exceptions.
    /// 
    /// Observations: We now have waits and enough retries: all calls now succeed!  Yay!
    /// But we kind-a had to guess how many retries would be enough before the server responded again ...
    /// (and we're hammering that server with retries)
    /// </summary>
    public static class Demo03_WaitAndRetryNTimes_WithEnoughRetries
    {
        public static void Execute()
        {
            Console.WriteLine(MethodBase.GetCurrentMethod().DeclaringType.Name);
            Console.WriteLine("=======");

            // Let's call a web api service to make repeated requests to a server. 
            // The service is programmed to fail after 3 requests in 5 seconds.

            var client = new WebClient();
            int eventualSuccesses = 0;
            int retries = 0;
            int eventualFailures = 0;
            // Define our policy:
            var policy = Policy.Handle<Exception>().WaitAndRetry(
                retryCount: 15, // Retry up to 15 times! - should be enough that we eventually succeed.
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(200), // Wait 200ms between each try.
                onRetry: (exception, calculatedWaitDuration) => // Capture some info for logging!
            {
                // This is your new exception handler! 
                // Tell the user what they've won!
                ConsoleHelper.WriteLineInColor("Policy logging: " + exception.Message, ConsoleColor.Yellow);
                retries++;

            });

            int i = 0;
            // Do the following until a key is pressed
            while (!Console.KeyAvailable)
            {
                i++;

                try
                {
                    // Retry the following call according to the policy - 15 times.
                    policy.Execute(() =>
                    {
                        // This code is executed within the Policy 

                        // Make a request and get a response
                        var msg = client.DownloadString(Configuration.WEB_API_ROOT + "/api/values/" + i.ToString());

                        // Display the response message on the console
                        ConsoleHelper.WriteLineInColor("Response : " + msg, ConsoleColor.Green);
                        eventualSuccesses++;
                    });
                }
                catch (Exception e)
                {
                    ConsoleHelper.WriteLineInColor("Request " + i + " eventually failed with: " + e.Message, ConsoleColor.Red);
                    eventualFailures++;
                }

                // Wait half second before the next request.
                Thread.Sleep(500);
            }

            Console.WriteLine("");
            Console.WriteLine("Total requests made                 : " + i);
            Console.WriteLine("Requests which eventually succeeded : " + eventualSuccesses);
            Console.WriteLine("Retries made to help achieve success: " + retries);
            Console.WriteLine("Requests which eventually failed    : " + eventualFailures);

        }
    }
}
