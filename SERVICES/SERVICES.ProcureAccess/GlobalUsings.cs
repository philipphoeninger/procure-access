global using DAL.ProcureAccess.Repos;
global using DAL.ProcureAccess.Repos.Base;
global using DAL.ProcureAccess.Repos.Interfaces;

global using MODELS.ProcureAccess.Entities;
global using MODELS.ProcureAccess.Entities.Base;

global using Microsoft.AspNetCore.Builder;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

global using Serilog;
global using Serilog.Context;
global using Serilog.Core.Enrichers;
global using Serilog.Events;
global using Serilog.Sinks.MSSqlServer;

global using System.Data;
global using System.Runtime.CompilerServices;
global using System.Net.Http.Headers;
global using System.Net.Http.Json;
global using System.Text;
global using System.Text.Json;

global using SERVICES.ProcureAccess.DataServices;
//global using SERVICES.ProcureAccess.DataServices.Api;
//global using SERVICES.ProcureAccess.DataServices.Api.Base;
//global using SERVICES.ProcureAccess.DataServices.Interfaces;
global using SERVICES.ProcureAccess.Logging.Interfaces;
global using SERVICES.ProcureAccess.Logging.Configuration;
global using SERVICES.ProcureAccess.Logging.Settings;
global using SERVICES.ProcureAccess.Utilities;
