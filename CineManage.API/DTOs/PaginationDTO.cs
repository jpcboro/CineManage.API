namespace CineManage.API.DTOs
{
    public class PaginationDTO
    {
        public int PageNumber { get; set; } = 1;

        private int maxAmoutRecordsPerPage = 50;

        private int recordsPerPage = 10;

        public int RecordsPerPage
        {
            get => recordsPerPage;

            set
            {
                recordsPerPage = value > maxAmoutRecordsPerPage ? 
                                   maxAmoutRecordsPerPage : value;
            }
        }
    }
}
