global using Amazon;
global using Amazon.Lambda.Core;
global using Amazon.Lambda.SQSEvents;
global using Amazon.SimpleEmail.Model;

global using dotenv.net.Utilities;

global using Microsoft.Extensions.Logging;

global using Sqs.Email.Forwarder.Abstractions;
global using Sqs.Email.Forwarder.Extensions;
global using Sqs.Email.Forwarder.Models;
global using Sqs.Email.Forwarder.Providers;
global using Sqs.Email.Forwarder.Services;

global using System.Net;
global using System.Text.Json;