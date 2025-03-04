using Azure;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Qualifications
{
    public class PaginationViewModel
    {
        public PaginationViewModel()
        {                
        }

        public PaginationViewModel(int totalRecords, int skip, int take)
        {
            TotalRecords = totalRecords;
            CurrentPage = skip > 0 ? ((int)Math.Floor(skip / (double)take) + 1) : 1;
            RecordsPerPage = take;
        }

        public int FirstPage { get; set; } = 1;        
        public int PreviousPage
        {
            get
            {
                return CurrentPage > 1 ? CurrentPage - 1 : 0;
            }
        }
        public int NextPage
        {
            get
            {
                return CurrentPage + 1;
            }
        }
        public int CurrentPage { get; set; }
        public int TotalPages 
        { 
            get
            {
                return RecordsPerPage > 0 ? (int)Math.Ceiling(TotalRecords / (double)RecordsPerPage) : 0;
            }
        }

        //value n in text 'viewing x - y of n records''
        public int TotalRecords { get; set; } = 0;
        
        public int RecordsPerPage { get; set; } = 10;

        //value x in text 'viewing x - y of n records''
        public int StartRecord 
        { 
            get
            {
                return 1 + ((CurrentPage - 1) * RecordsPerPage);
            }
        }

        //value y in text 'viewing x - y of n records''
        public int EndRecord
        {
            get
            {
                return CurrentPage * RecordsPerPage;
            }
        }

        public bool HasPreviousPage
        {
            get
            {
                return CurrentPage > 1;
            }
        }

        public bool HasNextPage
        {
            get
            {
                return CurrentPage < TotalPages;
            }
        }

        public bool ShowExtendedPreviousNavigation
        {
            get
            {
                return CurrentPage > 3;
            }
        }

        public bool ShowExtendedNextNavigation 
        {
            get 
            {
                return CurrentPage <= (TotalPages - 3);
            }
        }
    }
}