using BlazorAssistant.Storage.Exceptions;
using BlazorAssistant.Storage.Utils;
using Microsoft.JSInterop;
using System.Text.Json;

namespace BlazorAssistant.Storage.Services
{
    public interface ILocalStorageService
    {
        /// <summary>
        /// Try to get a stored value from Local Storage.
        /// </summary>
        /// <typeparam name="T">The type of the stored value to get.</typeparam>
        /// <param name="key">The key of the stored value.</param>
        /// <param name="isCompressed">Flag if the value is compressed using Gzip.</param>
        /// <returns>Null if the value was manipulated or couldn't be deserialized. Else the expected value.</returns>
        /// <exception cref="LocalStorageKeyNotFoundException">When the provided key was not found in Local Storage.</exception>
        /// <exception cref="EmptyLocalStorageValueException">If the key was found but the value was empty in Local Storage.</exception>
        ValueTask<T?> GetAsync<T>(string key, bool isCompressed = true);

        /// <summary>
        /// Try get a stored string value from Local Storage.
        /// </summary>
        /// <param name="key">The key of the stored value.</param>
        /// <param name="isCompressed">Flag if the value is compressed using Gzip.</param>
        /// <returns>The stored value in Local Storage. Decompressed if the <paramref name="isCompressed"/> is true.</returns>
        /// <exception cref="LocalStorageKeyNotFoundException">When the provided key was not found in Local Storage.</exception>
        /// <exception cref="EmptyLocalStorageValueException">If the key was found but the value was empty in Local Storage.</exception>
        ValueTask<string> GetAsStringAsync(string key, bool isCompressed = true);

        /// <summary>
        /// Sets a value in Local Storage from an object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be serialized and set in Local Storage.</typeparam>
        /// <param name="key">The key to represent the value in Local Storage.</param>
        /// <param name="value">The actual value to be set in Local Storage.</param>
        /// <param name="compress">Flag if the value should be compressed using Gzip.</param>
        /// <exception cref="ArgumentException">If the provided value could not be serialized correctly.</exception>
        ValueTask SetAsync<T>(string key, T value, bool compress = true);

        /// <summary>
        /// Sets a string value in Local Storage.
        /// </summary>
        /// <param name="key">The key to represent the value in Local Storage.</param>
        /// <param name="value">The actual value to be set in Local Storage.</param>
        /// <param name="compress">Flag if the value should be compressed using Gzip.</param>
        /// <exception cref="ArgumentException">If the provided string value is null or empty.</exception>
        ValueTask SetFromStringAsync(string key, string value, bool compress = true);

        /// <summary>
        /// Removes a record from Local Storage by a key if it exists.
        /// </summary>
        /// <param name="key">The key of the record to be removed in Local Storage.</param>
        ValueTask RemoveAsync(string key);

        /// <summary>
        /// Clears the entire Local Storage.
        /// </summary>
        ValueTask ClearAsync();
    }

    public class LocalStorageService : ILocalStorageService
    {
        private readonly IJSRuntime _js;

        public LocalStorageService(IJSRuntime js)
        {
            _js = js;
        }

        /// <inheritdoc />
        public async ValueTask ClearAsync()
        {
            try
            {
                await _js.InvokeVoidAsync(JSFunctions.CLEAR_LOCAL_STORAGE);
            }
            catch
            {
                throw;
            }
        }

        /// <inheritdoc />
        public async ValueTask<string> GetAsStringAsync(string key, bool isCompressed = true)
        {
            try
            {
                return await GetValueAsync(key, isCompressed);
            }
            catch
            {
                throw;
            }
        }

        /// <inheritdoc />
        public async ValueTask<T?> GetAsync<T>(string key, bool isCompressed = true)
        {
            try
            {
                string value = await GetValueAsync(key, isCompressed);
                return JsonSerializer.Deserialize<T>(value);
            }
            catch
            {
                throw;
            }
        }

        /// <inheritdoc />
        public async ValueTask RemoveAsync(string key)
        {
            try
            {
                await _js.InvokeVoidAsync(JSFunctions.REMOVE_FROM_LOCAL_STORAGE, key);
            }
            catch
            {
                throw;
            }
        }

        /// <inheritdoc />
        public async ValueTask SetAsync<T>(string key, T value, bool compress = true)
        {
            try
            {
                string? serialized = JsonSerializer.Serialize(value);
                if (string.IsNullOrEmpty(serialized))
                    throw new ArgumentException($"Failed to serialize provided value of type {typeof(T)}.");

                string localStorageValue = compress
                    ? await serialized.Gzip()
                    : serialized;

                await _js.InvokeVoidAsync(JSFunctions.SET_IN_LOCAL_STORAGE, key, localStorageValue);
            }
            catch
            {
                throw;
            }
        }

        /// <inheritdoc />
        public async ValueTask SetFromStringAsync(string key, string value, bool compress = true)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException($"Provided value can't be null or empty.");

                string localStorageValue = compress
                    ? await value.Gzip()
                    : value;

                await _js.InvokeVoidAsync(JSFunctions.SET_IN_LOCAL_STORAGE, key, localStorageValue);
            }
            catch
            {
                throw;
            }
        }

        /// <inheritdoc />
        private async ValueTask<string> GetValueAsync(string key, bool isCompressed)
        {
            string value = await _js.InvokeAsync<string>(JSFunctions.GET_FROM_LOCAL_STORAGE, key)
                    ?? throw new LocalStorageKeyNotFoundException(key);

            if (value.Length == 0)
                throw new EmptyLocalStorageValueException(key);

            return isCompressed
                ? await value.UnGzip()
                : value;
        }
    }
}
