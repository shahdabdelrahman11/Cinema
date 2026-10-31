using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Cinema.Services
{
    public static class ServiceResultExtensions
    {
        public static void AddServiceErrors(this ModelStateDictionary modelState, ServiceResult result)
        {
            foreach (var (key, message) in result.Errors)
                modelState.AddModelError(key, message);
        }
    }
}