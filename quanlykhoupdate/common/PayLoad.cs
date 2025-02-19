using System.Net;

namespace quanlykhoupdate.common
{
    public class PayLoad<T>
    {
        public PayLoad(T? content = default, int errCode = 0, object errMsg = null)
        {

            if (content == null && errMsg == null)
                throw new Exception($"At least {nameof(content)} or error message should has value");

            Content = content;
            ErrorCode = errCode;
            Error = errMsg;
            Success = content != null;
        }

        public static PayLoad<T> NotFound(string message = "")
        {
            return new PayLoad<T>(default, (int)HttpStatusCode.NotFound, string.IsNullOrEmpty(message) ? "Item not found!" : message);
        }

        public static PayLoad<T> BadRequest(string message = "")
        {
            return new PayLoad<T>(default, (int)HttpStatusCode.BadRequest, string.IsNullOrEmpty(message) ? "Bad Request!" : message);
        }

        public static PayLoad<T> Dublicated(T data, string message = "")
        {
            return new PayLoad<T>(data, (int)HttpStatusCode.Ambiguous, string.IsNullOrEmpty(message) ? "Duplicated data!" : message);
        }

        public static PayLoad<T> Successfully(T data, string message = "")
        {
            return new PayLoad<T>(data, (int)HttpStatusCode.OK, string.IsNullOrEmpty(message) ? "OK" : message);
        }

        public static PayLoad<string> Successfully(string message = "")
        {
            return new PayLoad<string>("OK", (int)HttpStatusCode.OK, string.IsNullOrEmpty(message) ? "OK" : message);
        }
        public static PayLoad<T> ValidateModel(T data, object message = null)
        {
            return new PayLoad<T>(data, (int)HttpStatusCode.BadRequest, message);
        }
        public static PayLoad<List<T>> SuccessfullyLists(List<T> newUser)
        {
            return new PayLoad<List<T>>(newUser, (int)HttpStatusCode.ExpectationFailed, "OK");
        }

        public static PayLoad<T> CreatedFail(string message = "", object data = null)
        {
            return new PayLoad<T>((T?)data, (int)HttpStatusCode.ExpectationFailed, string.IsNullOrEmpty(message) ? "Created Fail!" : message);
        }

        public static PayLoad<T> UpdatedFail(string message = "", object data = null)
        {
            return new PayLoad<T>((T?)data, (int)HttpStatusCode.ExpectationFailed, string.IsNullOrEmpty(message) ? "Updated Fail!" : message);
        }

        public static PayLoad<T> DeletedFail(string message = "", object data = null)
        {
            return new PayLoad<T>((T?)data, (int)HttpStatusCode.ExpectationFailed, string.IsNullOrEmpty(message) ? "deleted Fail!" : message);
        }

        public static PayLoad<T> RequestInvalid(string message = "", object data = null)
        {
            return new PayLoad<T>((T?)data, (int)HttpStatusCode.BadRequest, string.IsNullOrEmpty(message) ? "Request invalid!" : message);
        }

        public static PayLoad<T> ErrorInProcessing(string message = "", object data = null)
        {
            return new PayLoad<T>((T?)data, (int)HttpStatusCode.InternalServerError, string.IsNullOrEmpty(message) ? "Error in processing!" : message);
        }

        public static PayLoad<T> DataValidationFail(string message = "", object data = null)
        {
            return new PayLoad<T>((T?)data, (int)HttpStatusCode.ExpectationFailed, string.IsNullOrEmpty(message) ? "Data Validation Fail!" : message);
        }
        public bool Success { get; set; }
        public object Error { get; set; }
        public int ErrorCode { get; set; }
        public T? Content { get; set; }
    }
}
