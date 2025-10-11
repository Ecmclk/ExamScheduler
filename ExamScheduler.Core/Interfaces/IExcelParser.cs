using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamScheduler.Core.Interfaces
{
    public interface IExcelParser<T>
    {
        Task<ParseResult<T>> ParseAsync(string filePath);
    }

    public class ParseResult<T>
    {
        public List<T> Data { get; set; } = new();
        public List<ParseError> Errors { get; set; } = new();
        public bool IsSuccess => Errors.Count == 0;
    }

    public class ParseError
    {
        public int RowNumber { get; set; }
        public string Message { get; set; }
    }
}

public class ParseError
{
    public int RowNumber { get; set; }
    public string Message { get; set; }
}
