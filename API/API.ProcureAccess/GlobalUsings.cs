global using API.ProcureAccess.ApiVersionSupport;
global using API.ProcureAccess.Swagger;
global using API.ProcureAccess.Swagger.Models;
global using API.ProcureAccess.Controllers;
global using API.ProcureAccess.Controllers.Base;
global using API.ProcureAccess.Controllers.Identity;
global using API.ProcureAccess.Controllers.Models;
global using API.ProcureAccess.Extensions;
//global using API.ProcureAccess.Filters;
//global using API.ProcureAccess.Security;

global using DAL.ProcureAccess.EFStructures;
global using DAL.ProcureAccess.Initialization;
global using DAL.ProcureAccess.Exceptions;
global using DAL.ProcureAccess.Repos;
global using DAL.ProcureAccess.Repos.Base;
global using DAL.ProcureAccess.Repos.Interfaces;

global using MODELS.ProcureAccess.Entities;
global using MODELS.ProcureAccess.Entities.Base;
global using MODELS.ProcureAccess.Settings;

global using SERVICES.ProcureAccess.DataServices;
global using SERVICES.ProcureAccess.Utilities;
global using SERVICES.ProcureAccess.Logging.Interfaces;
global using SERVICES.ProcureAccess.Logging.Configuration;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.ApiExplorer;
global using Asp.Versioning;
global using Asp.Versioning.ApiExplorer;
global using Microsoft.AspNetCore.Mvc.Filters;
global using Microsoft.Extensions.Options;
global using Microsoft.OpenApi.Any;
global using Microsoft.OpenApi.Models;
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc.Authorization;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Identity.Data;
global using Microsoft.IdentityModel.Tokens;

global using Swashbuckle.AspNetCore.Annotations;
global using Swashbuckle.AspNetCore.SwaggerGen;

global using System.Reflection;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Net.Http.Headers;
global using System.Security.Claims;
global using System.Text;
global using System.Text.Encodings.Web;
global using System.IdentityModel.Tokens.Jwt;
