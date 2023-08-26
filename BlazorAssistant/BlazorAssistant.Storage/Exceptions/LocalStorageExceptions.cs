namespace BlazorAssistant.Storage.Exceptions
{
    public class LocalStorageKeyNotFoundException : Exception
    {
        public LocalStorageKeyNotFoundException(string key) : base($"Key {key} was not found in Local Storage.") { }
    }

    public class EmptyLocalStorageValueException : Exception
    {
        public EmptyLocalStorageValueException(string key) : base($"Value belonging to key {key} is empty.") { }
    }
}
