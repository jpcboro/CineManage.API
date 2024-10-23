namespace CineManage.API.DTOs;

public class MoviesFilterDTO
{
    public int PageNumber { get; set; }
    public int RecordsPerPage { get; set; }

    internal PaginationDTO PaginationDto
    {
        get
        {
            return new PaginationDTO()
            {
                PageNumber = PageNumber,
                RecordsPerPage = RecordsPerPage
            };

        }
    }

    public string? Title { get; set; }
    public int GenreId { get; set; }
    public bool IsNowShowing { get; set; }
    public bool IsUpcomingMovie { get; set; }
}