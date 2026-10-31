using System.Collections.Generic;

namespace Cinema.Services
{
    public class ServiceResult
    {
        public List<(string Key, string Message)> Errors { get; } = new();

        public bool Succeeded => Errors.Count == 0;

        public void AddError(string key, string message) => Errors.Add((key, message));
    }
}
