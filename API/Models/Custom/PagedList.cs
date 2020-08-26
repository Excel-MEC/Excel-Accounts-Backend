using System;
using System.Collections.Generic;
using System.Linq;

namespace API.Models.Custom
{
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }

        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        private PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            if(pageSize == 0)
            {
                PageSize = count;
                TotalPages = 1;
            }
            else
            {
                PageSize = pageSize;
                TotalPages = (int) Math.Ceiling(count / (double) pageSize);
            }
            CurrentPage = pageNumber;
            AddRange(items);
        }

        public static PagedList<T> ToPagedList(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var items = pageSize > 0 ? source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList() : source.ToList();

            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}