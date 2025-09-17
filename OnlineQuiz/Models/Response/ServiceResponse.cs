namespace OnlineQuiz.Models.Response
{
    public class ServiceResponse<T>
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public ServiceResponse() { }

        public ServiceResponse(T data)
        {
            Data = data;
        }

        public ServiceResponse(string errorMessage)
        {
            Success = false;
            Message = errorMessage;
            Data = default(T);
        }
    }

    public class ServiceResponse
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();

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