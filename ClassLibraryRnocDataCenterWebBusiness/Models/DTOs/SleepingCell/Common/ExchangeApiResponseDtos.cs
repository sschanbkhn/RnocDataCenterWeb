using System;
using System.Collections.Generic;

namespace ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Common
{
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public IEnumerable<string> Errors { get; set; } = new List<string>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string RequestId { get; set; } = string.Empty;
    }

    public class PagedResultDto<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    public class PageRequestDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = string.Empty;
        public string SortDirection { get; set; } = "asc";
        public string SearchTerm { get; set; } = string.Empty;
    }

    public class ErrorDto
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public object? AttemptedValue { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class FilterRequestDto
    {
        public string SearchTerm { get; set; } = string.Empty;
        public Dictionary<string, object> Filters { get; set; } = new();
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public IEnumerable<string> Vendors { get; set; } = new List<string>();
        public IEnumerable<string> Provinces { get; set; } = new List<string>();
    }

    public class AuditLogDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string IpAddress { get; set; } = string.Empty;
        public object? AdditionalData { get; set; }
    }
}