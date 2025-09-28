namespace OnlineQuiz.Models.Response
{
    public class ServiceResponse<T>
    {
        public bool Success { get; set; } = true;
        public string? Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = [];

        public ServiceResponse() { }

        public ServiceResponse(T? data)  // allow null payloads
        {
            Data = data;
            Success = true;
        }

        public ServiceResponse(string errorMessage)
        {
            Success = false;
            Message = errorMessage;
            Data = default;
        }

        public static ServiceResponse<T> Fail(string? message = null)
        {
            return new ServiceResponse<T>
            {
                Success = false,
                Message = message,
                Data = default
            };
        }
    }

    public class ServiceResponse
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = [];

        public ServiceResponse() { }

        public ServiceResponse(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Success = false;
                Message = message;
            }
        }
    }
}