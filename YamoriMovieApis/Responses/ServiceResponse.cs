namespace YamoriMovieApis.Responses
{
    public class ServiceResponse<T>
    {
        public int StatusCode { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T Data { get; set; }

        // successful method
        public ServiceResponse<T> Successful(string message) => new ServiceResponse<T> { StatusCode = 200, Message = message, Data = Data, Success = true };
        // failure method
        public ServiceResponse<T> Failure(string message) => new ServiceResponse<T> { StatusCode = 500, Message = message, Data = Data, Success = false };

    }


}
