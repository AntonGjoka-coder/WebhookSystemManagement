using System.Reflection;
using Application.Exceptions;
using ManagementSystemAPI.Controllers.OData;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace WebAPI.OData;

public static class Startup
{
    internal static IMvcBuilder ConfigureOData(this IMvcBuilder builder) =>
        builder.AddOData(options => options.EnableQueryFeatures().AddRouteComponents("oData", GetEdmModel()));

    internal static IEdmModel GetEdmModel()
    {
        ODataConventionModelBuilder builder = new();
        builder.EnableLowerCamelCase();
        builder.RegisterEntrySetsViaReflection();
        return builder.GetEdmModel();
    }
    private static IEnumerable<Type> GetODataControllers() =>
        Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => typeof(ODataBaseController).IsAssignableFrom(type) && !type.IsAbstract);

    private static void RegisterEntrySetsViaReflection(this ODataConventionModelBuilder builder)
    {
        foreach (var controller in GetODataControllers())
        {
            var actions = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(method => IsTaskOfIQueriable(method.ReturnType));
            foreach (var action in actions)
            {
               var retype = action.ReturnType.GetGenericArguments()[0].GetGenericArguments()[0];
               var routeAttribute = action.GetCustomAttribute<HttpMethodAttribute>();

               if (routeAttribute != null)
               {
                   var entitySetName = routeAttribute.Template;
                   _ = entitySetName ?? throw new InternalServerException("Faild to register OData endpoint");
                   
                   var genericMethod = typeof(ODataConventionModelBuilder)
                       .GetMethod("EntitySet", new Type[] {typeof(string)})
                       .MakeGenericMethod(retype);

                   genericMethod.Invoke(builder, new object[] { entitySetName });
               }
            }
        }
    }

    private static bool IsTaskOfIQueriable(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var genericArguments = type.GetGenericArguments()[0];
            return genericArguments.IsGenericType && 
                   genericArguments.GetGenericTypeDefinition() == typeof(IQueryable<>);
        }

        return false;
    }
}