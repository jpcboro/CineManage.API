using Microsoft.EntityFrameworkCore;


namespace CineManage.API.Utilities
{
    public static class HttpContextExtensions
    {
        public async static Task InserPaginationParametersInHeader<T>(this HttpContext httpContext,
            IQueryable<T> queryable)
        {
            if (httpContext is null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            double count = await queryable.CountAsync();

            httpContext.Response.Headers.Append(Constants.TotalRecordsCount, count.ToString());
        }
    }
}
