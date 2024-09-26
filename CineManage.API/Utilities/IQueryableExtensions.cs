using CineManage.API.DTOs;

namespace CineManage.API.Utilities
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> Paginate<T>(this IQueryable<T> queryable,
            PaginationDTO pagination)
        {
            return queryable
                .Skip((pagination.PageNumber - 1) * pagination.RecordsPerPage)
                .Take(pagination.RecordsPerPage);
        }
    }
}
