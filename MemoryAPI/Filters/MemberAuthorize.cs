using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Memory.API.Services;
using Microsoft.AspNetCore.Mvc;
// See https://stackoverflow.com/questions/39181390/how-do-i-add-a-parameter-to-an-action-filter-in-asp-net
// For filter DI injection.

namespace Memory.API.Filters
{
    public class MemberAuthorize : TypeFilterAttribute
    {
        // Attributes don't use DI.
        public MemberAuthorize() : base(typeof(MemberAuthorizeImpl))
        {
        }
        // Here in the Filter implementation was can use DI
        // in the contructor.
        private class MemberAuthorizeImpl : IAuthorizationFilter{
            private IMemoryRepository _memoryRepository;
            public MemberAuthorizeImpl(IMemoryRepository memoryRepository)
            {
                _memoryRepository = memoryRepository;
            }
            public void OnAuthorization(AuthorizationFilterContext context)
            {
                var currentId = _memoryRepository.GetUserId(context.HttpContext.User);
                var requestId = (string)context.RouteData.Values["userId"];
                var isAdmin = context.HttpContext.User.IsInRole("Admin");
                if(requestId != "self" && requestId != currentId && !isAdmin)
                {
                    context.Result = new UnauthorizedResult();
                }
            }
        } 
    }
}