using System;
using System.Collections.Generic;

namespace Neo4jOgm.Domain
{
    public class PageRequest
    {
        /**
         * Start from 1
         */
        public int Page { get; }

        /**
         * Start from 1
         */
        public int Size { get; }

        public Sort Sort { get; set; } = new();

        public PageRequest(int page, int size)
        {
            if (page < 1 || size < 1)
            {
                throw new Neo4JException("Illegal page or size, they must be greater than 1");
            }

            Page = page;
            Size = size;
        }

        public long Offset()
        {
            return (Page - 1) * (long) Size;
        }
    }

    public class Page<T>
    {
        public int CurrentPage { get; }

        public int PageSize { get; }

        public IList<T> Items { get; init; }

        public int TotalPages => Items.Count == 0 ? 1 : (int) Math.Ceiling(TotalItems / (double) PageSize);

        public long TotalItems { get; init; }

        public Page(PageRequest r)
        {
            CurrentPage = r.Page;
            PageSize = r.Size;
        }
    }
}