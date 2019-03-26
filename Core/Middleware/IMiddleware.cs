using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Vacation24.Middleware {
    public interface IMiddleware
    {
        Task ExecuteAsync(HttpContext context, Func<Task> next);
    }
}