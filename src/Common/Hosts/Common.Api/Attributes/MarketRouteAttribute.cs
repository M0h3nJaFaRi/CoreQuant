using Common.Api.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Common.Api.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class MarketRouteAttribute : RouteAttribute
{
    public MarketRouteAttribute(string controller) : 
           base($"api/v{{version:apiVersion}}/markets/{controller.Trim('/')}") 
    { 
    
    }

    public MarketRouteAttribute(MarketType marketType, string controller) :
           base($"api/v{{version:apiVersion}}/markets/{marketType.ToString().ToLowerInvariant().Trim('/')}/{controller.Trim('/')}") 
    {
        
    }

    public MarketRouteAttribute(MarketType marketType) :
           base($"api/v{{version:apiVersion}}/markets/{marketType.ToString().ToLowerInvariant().Trim('/')}")
    {

    }
}