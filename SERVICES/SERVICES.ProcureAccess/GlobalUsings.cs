global using AutoMapper;

global using DAL.ProcureAccess.EFStructures;
global using DAL.ProcureAccess.Exceptions;
global using DAL.ProcureAccess.Repos;
global using DAL.ProcureAccess.Repos.Base;
global using DAL.ProcureAccess.Repos.Interfaces;

global using MODELS.ProcureAccess.Entities;
global using MODELS.ProcureAccess.Entities.Authorization;
global using MODELS.ProcureAccess.Entities.Base;
global using MODELS.ProcureAccess.Entities.Dto;
global using MODELS.ProcureAccess.Entities.Identity;
global using MODELS.ProcureAccess.Entities.Mapping;
global using MODELS.ProcureAccess.Entities.Requests;
global using MODELS.ProcureAccess.Settings;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Mvc.Filters;
global using Microsoft.Extensions.Options;
// global using Microsoft.OpenApi.Any;
// global using Microsoft.OpenApi.Models;
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Identity.Data;
global using Microsoft.AspNetCore.Identity.UI.Services;
global using Microsoft.AspNetCore.Mvc.Authorization;
global using Microsoft.IdentityModel.Tokens;

global using Microsoft.EntityFrameworkCore;
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
global using System.IdentityModel.Tokens.Jwt;
global using System.Runtime.CompilerServices;
global using System.Net.Http.Headers;
global using System.Net.Http.Json;
global using System.Text;
global using System.Text.Json;
global using System.Security.Claims;

global using SERVICES.ProcureAccess.DataServices;
global using SERVICES.ProcureAccess.DataServices.Base;
global using SERVICES.ProcureAccess.DataServices.Configuration;
global using SERVICES.ProcureAccess.DataServices.Interfaces;
//global using SERVICES.ProcureAccess.DataServices.Api;
//global using SERVICES.ProcureAccess.DataServices.Api.Base;
global using SERVICES.ProcureAccess.Logging.Interfaces;
global using SERVICES.ProcureAccess.Logging.Configuration;
global using SERVICES.ProcureAccess.Logging.Settings;
global using SERVICES.ProcureAccess.Utilities;

global using MailKit.Net.Smtp;
global using MailKit.Security;
global using MimeKit;
