

using Microsoft.AspNetCore.Authorization;

namespace ServerLibrary.MiddleWare
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class HasPermissionAttribute : Attribute
    {
        public string Action { get; }
        public string Resource { get; }

        public HasPermissionAttribute(string action, string resource)
        {
            Action = action;
            Resource = resource;
        }
    }

}
